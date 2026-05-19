// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Erstellt Transaktionen mit ihren Buchungssätzen atomar.
    ///
    /// Eine Transaktion = ein Kontoauszugseintrag (Geld bewegt sich physisch).
    /// Der Betrag wird aus den Positionen berechnet.
    ///
    /// Unterstützte Positionstypen:
    ///   Mieten               — Kaltmiete + NK-VZ, legt Sollstellung an falls fehlend
    ///   BetriebskostenEingang — legt Rechnung + Forderungs- und Zahlungs-Buchungssatz an
    ///   Erhaltungsaufwendungen — Aufwands-Buchungssatz gegen Zahlungskonto
    ///   Sonstige             — direkter Buchungssatz mit expliziten Soll/Haben-Konten
    /// </summary>
    public class TransaktionBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public TransaktionBuchungsService(SaverwalterContext ctx) => _ctx = ctx;

        // ── Input-DTOs ──────────────────────────────────────────────────────────

        public class MietzahlungsInput
        {
            public int VertragId { get; set; }
            public DateOnly BetreffenderMonat { get; set; }
            public decimal Kaltmiete { get; set; }
            public decimal NkVorauszahlung { get; set; }
        }

        public class BetriebskostenEingangInput
        {
            public int? ExistingBetriebskostenrechnungId { get; set; }
            public int UmlageId { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly RechnungsDatum { get; set; }
            public decimal Betrag { get; set; }
            public string? Notiz { get; set; }
        }

        public class ErhaltungsaufwendungsInput
        {
            public int? ExistingErhaltungsaufwendungId { get; set; }
            public int WohnungId { get; set; }
            public decimal Betrag { get; set; }
            public string? Beschreibung { get; set; }
        }

        public class SonstigerBuchungssatzInput
        {
            public decimal Betrag { get; set; }
            public string? Beschreibung { get; set; }
        }

        public class TransaktionsInput
        {
            public decimal Betrag { get; set; }
            public DateOnly Zahlungsdatum { get; set; }
            public int? ZahlerId { get; set; }
            public int? ZahlungsempfaengerId { get; set; }
            public string Verwendungszweck { get; set; } = string.Empty;
            public string? Notiz { get; set; }
            public List<MietzahlungsInput> Mieten { get; set; } = [];
            public List<BetriebskostenEingangInput> BetriebskostenEingaenge { get; set; } = [];
            public List<ErhaltungsaufwendungsInput> Erhaltungsaufwendungen { get; set; } = [];
            public List<SonstigerBuchungssatzInput> Sonstige { get; set; } = [];
        }

        // ── Hauptmethode ─────────────────────────────────────────────────────────

        public async Task<Transaktion> BucheAsync(TransaktionsInput input)
        {
            var totalBetrag =
                input.Mieten.Sum(m => m.Kaltmiete + m.NkVorauszahlung) +
                input.BetriebskostenEingaenge.Sum(b => b.Betrag) +
                input.Erhaltungsaufwendungen.Sum(e => e.Betrag) +
                input.Sonstige.Sum(s => s.Betrag);

            if (totalBetrag <= 0)
                throw new ArgumentException("Mindestens eine Position mit Betrag > 0 erforderlich.");

            if (Math.Abs(input.Betrag - totalBetrag) > 0.005m)
                throw new ArgumentException(
                    $"Transaktionsbetrag ({input.Betrag:C}) stimmt nicht mit der Summe der Positionen ({totalBetrag:C}) überein.");

            var transaktion = new Transaktion
            {
                Zahlungsdatum = input.Zahlungsdatum,
                Betrag = totalBetrag,
                Verwendungszweck = input.Verwendungszweck,
                Notiz = input.Notiz
            };

            if (input.ZahlerId.HasValue)
                transaktion.Zahler = await _ctx.Bankkontos
                    .Include(b => b.BuchungsKonto)
                    .FirstOrDefaultAsync(b => b.BankkontoId == input.ZahlerId.Value)
                    ?? throw new ArgumentException($"Zahler {input.ZahlerId} nicht gefunden.");

            if (input.ZahlungsempfaengerId.HasValue)
                transaktion.Zahlungsempfaenger = await _ctx.Bankkontos
                    .Include(b => b.BuchungsKonto)
                    .FirstOrDefaultAsync(b => b.BankkontoId == input.ZahlungsempfaengerId.Value)
                    ?? throw new ArgumentException($"Zahlungsempfänger {input.ZahlungsempfaengerId} nicht gefunden.");

            if (input.Erhaltungsaufwendungen.Count > 0 && transaktion.Zahler is null)
                throw new ArgumentException("Erhaltungsaufwendungen erfordern ein Zahlerkonto.");

            if (input.Sonstige.Count > 0 && (transaktion.Zahler is null || transaktion.Zahlungsempfaenger is null))
                throw new ArgumentException("Sonstige Buchungssätze erfordern Zahler und Zahlungsempfänger.");

            _ctx.Transaktionen.Add(transaktion);

            foreach (var miete in input.Mieten)
                await ErstelleMietzahlungAsync(miete, transaktion);

            foreach (var bk in input.BetriebskostenEingaenge)
            {
                var satz = await ErstelleBetriebskostenEingangAsync(bk, input.Zahlungsdatum);
                satz.Notiz = input.Notiz;
                satz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(satz);
            }

            foreach (var ea in input.Erhaltungsaufwendungen)
            {
                var satz = await ErstelleErhaltungsaufwendungAsync(ea, input.Zahlungsdatum, transaktion);
                satz.Notiz = input.Notiz;
                satz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(satz);
            }

            foreach (var sonstiger in input.Sonstige)
            {
                var satz = ErstelleSonstigerBuchungssatz(sonstiger, input.Zahlungsdatum, transaktion);
                satz.Notiz = input.Notiz;
                satz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(satz);
            }

            await _ctx.SaveChangesAsync();
            return transaktion;
        }

        // ── Typisierte Hilfsmethoden ──────────────────────────────────────────────

        private async Task ErstelleMietzahlungAsync(MietzahlungsInput miete, Transaktion transaktion)
        {
            var vertrag = await _ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .FirstOrDefaultAsync(v => v.VertragId == miete.VertragId)
                ?? throw new ArgumentException($"Vertrag {miete.VertragId} nicht gefunden.");

            var monat = new DateOnly(miete.BetreffenderMonat.Year, miete.BetreffenderMonat.Month, 1);

            if (miete.Kaltmiete > 0)
            {
                var version = vertrag.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn)
                    ?? throw new InvalidOperationException($"Keine VertragVersion für {monat:MM/yyyy}.");

                if (miete.Kaltmiete > version.Grundmiete)
                    throw new InvalidOperationException(
                        $"Kaltmiete ({miete.Kaltmiete:C}) übersteigt Grundmiete ({version.Grundmiete:C}).");

                var sollZeile = FindeOderErstelleSollstellung(vertrag, monat, version);

                var kaltmieteSatz = new Buchungssatz(transaktion.Zahlungsdatum, $"Mietzahlung Kaltmiete {monat:MM/yyyy}");
                AddZeile(kaltmieteSatz, SollHaben.Soll, miete.Kaltmiete, vertrag.ZahlungsKonto);
                AddZeile(kaltmieteSatz, SollHaben.Haben, miete.Kaltmiete, vertrag.MietBuchungskonto);
                kaltmieteSatz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(kaltmieteSatz);

                var habenZeile = kaltmieteSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben);
                _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = sollZeile,
                    HabenZeile = habenZeile
                });
            }

            if (miete.NkVorauszahlung > 0)
            {
                var nkSatz = new Buchungssatz(transaktion.Zahlungsdatum, $"Mietzahlung NK-VZ {monat:MM/yyyy}");
                AddZeile(nkSatz, SollHaben.Soll, miete.NkVorauszahlung, vertrag.ZahlungsKonto);
                AddZeile(nkSatz, SollHaben.Haben, miete.NkVorauszahlung, vertrag.NkBuchungskonto);
                nkSatz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(nkSatz);
            }
        }

        /// <summary>
        /// Erstellt atomisch:
        ///   1. Betriebskostenrechnung-Entität
        ///   2. Forderungs-Buchungssatz: Haben Umlage.NkVerrechnungsKonto
        ///      (Soll fehlt absichtlich — kommt mit der BK-Jahresabrechnung)
        ///   3. Zahlungs-Buchungssatz (→ Transaktion): Soll NkVerrechnungsKonto / Haben ZahlungsKonto
        /// </summary>
        private async Task<Buchungssatz> ErstelleBetriebskostenEingangAsync(
            BetriebskostenEingangInput bk, DateOnly zahlungsdatum)
        {
            if (bk.ExistingBetriebskostenrechnungId.HasValue)
            {
                var rechnung = await _ctx.Betriebskostenrechnungen
                    .Include(b => b.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .Include(b => b.Umlage).ThenInclude(u => u.NkVerrechnungsKonto)
                    .Include(b => b.Umlage).ThenInclude(u => u.ZahlungsKonto)
                    .Include(b => b.Umlage).ThenInclude(u => u.Typ)
                    .FirstOrDefaultAsync(b => b.BetriebskostenrechnungId == bk.ExistingBetriebskostenrechnungId.Value)
                    ?? throw new ArgumentException($"Betriebskostenrechnung {bk.ExistingBetriebskostenrechnungId} nicht gefunden.");

                var zahlungsSatz = new Buchungssatz(
                    zahlungsdatum,
                    $"Zahlung Betriebskosten {rechnung.Umlage.Typ.Bezeichnung} {rechnung.BetreffendesJahr}");
                AddZeile(zahlungsSatz, SollHaben.Soll, bk.Betrag, rechnung.Umlage.NkVerrechnungsKonto);
                AddZeile(zahlungsSatz, SollHaben.Haben, bk.Betrag, rechnung.Umlage.ZahlungsKonto);

                var forderungsHabenZeile = rechnung.Buchungssatz.Buchungszeilen
                    .First(z => z.SollHaben == SollHaben.Haben);
                var zahlungsSollZeile = zahlungsSatz.Buchungszeilen
                    .First(z => z.SollHaben == SollHaben.Soll);
                _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = zahlungsSollZeile,
                    HabenZeile = forderungsHabenZeile
                });

                return zahlungsSatz;
            }

            var umlage = await _ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.ZahlungsKonto)
                .Include(u => u.Typ)
                .FirstOrDefaultAsync(u => u.UmlageId == bk.UmlageId)
                ?? throw new ArgumentException($"Umlage {bk.UmlageId} nicht gefunden.");

            var forderungsSatz = new Buchungssatz(
                bk.RechnungsDatum,
                $"Betriebskosten {umlage.Typ.Bezeichnung} {bk.BetreffendesJahr}");
            AddZeile(forderungsSatz, SollHaben.Haben, bk.Betrag, umlage.NkVerrechnungsKonto);
            _ctx.Buchungssaetze.Add(forderungsSatz);

            var neueRechnung = new Betriebskostenrechnung(bk.Betrag, bk.RechnungsDatum, bk.BetreffendesJahr)
            {
                Umlage = umlage,
                Buchungssatz = forderungsSatz,
                Notiz = bk.Notiz
            };
            _ctx.Betriebskostenrechnungen.Add(neueRechnung);

            var neuerZahlungsSatz = new Buchungssatz(
                zahlungsdatum,
                $"Zahlung Betriebskosten {umlage.Typ.Bezeichnung} {bk.BetreffendesJahr}");
            AddZeile(neuerZahlungsSatz, SollHaben.Soll, bk.Betrag, umlage.NkVerrechnungsKonto);
            AddZeile(neuerZahlungsSatz, SollHaben.Haben, bk.Betrag, umlage.ZahlungsKonto);

            _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = neuerZahlungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
                HabenZeile = forderungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben)
            });

            return neuerZahlungsSatz;
        }

        private async Task<Buchungssatz> ErstelleErhaltungsaufwendungAsync(
            ErhaltungsaufwendungsInput ea, DateOnly zahlungsdatum, Transaktion transaktion)
        {
            Wohnung wohnung;
            if (ea.ExistingErhaltungsaufwendungId.HasValue)
            {
                var existingEa = await _ctx.Erhaltungsaufwendungen
                    .Include(e => e.Wohnung).ThenInclude(w => w.AufwandsKonto)
                    .FirstOrDefaultAsync(e => e.ErhaltungsaufwendungId == ea.ExistingErhaltungsaufwendungId.Value)
                    ?? throw new ArgumentException($"Erhaltungsaufwendung {ea.ExistingErhaltungsaufwendungId} nicht gefunden.");
                wohnung = existingEa.Wohnung;
            }
            else
            {
                wohnung = await _ctx.Wohnungen
                    .Include(w => w.AufwandsKonto)
                    .FirstOrDefaultAsync(w => w.WohnungId == ea.WohnungId)
                    ?? throw new ArgumentException($"Wohnung {ea.WohnungId} nicht gefunden.");
            }

            var beschreibung = ea.Beschreibung is { Length: > 0 } b
                ? $"Erhaltungsaufwendung {b}"
                : $"Erhaltungsaufwendung {wohnung.Bezeichnung}";

            var satz = new Buchungssatz(zahlungsdatum, beschreibung);
            AddZeile(satz, SollHaben.Soll, ea.Betrag, wohnung.AufwandsKonto);
            AddZeile(satz, SollHaben.Haben, ea.Betrag, transaktion.Zahler!.BuchungsKonto);
            return satz;
        }

        private static Buchungssatz ErstelleSonstigerBuchungssatz(
            SonstigerBuchungssatzInput sonstiger, DateOnly zahlungsdatum, Transaktion transaktion)
        {
            var satz = new Buchungssatz(zahlungsdatum, sonstiger.Beschreibung ?? "Buchungssatz");
            AddZeile(satz, SollHaben.Soll, sonstiger.Betrag, transaktion.Zahlungsempfaenger!.BuchungsKonto);
            AddZeile(satz, SollHaben.Haben, sonstiger.Betrag, transaktion.Zahler!.BuchungsKonto);
            return satz;
        }

        // ── Hilfsfunktionen ───────────────────────────────────────────────────────

        private Buchungszeile FindeOderErstelleSollstellung(Vertrag vertrag, DateOnly monat, VertragVersion version)
        {
            var vorhandene = vertrag.MietBuchungskonto.Buchungszeilen
                .FirstOrDefault(z =>
                    z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                    && z.Buchungssatz.Buchungsdatum.Month == monat.Month);

            if (vorhandene != null) return vorhandene;

            var satz = new Buchungssatz(DritterWerktag(monat), $"Mietsoll {monat:MM/yyyy}");
            AddZeile(satz, SollHaben.Soll, version.Grundmiete, vertrag.MietBuchungskonto);
            AddZeile(satz, SollHaben.Haben, version.Grundmiete, vertrag.Wohnung.MietErtragskonto);
            _ctx.Buchungssaetze.Add(satz);
            return satz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll);
        }

        /// <summary>
        /// §556b Abs. 1 BGB: Miete ist spätestens am 3. Werktag fällig.
        /// Samstag gilt als Werktag. Feiertage werden nicht berücksichtigt (bundeslandabhängig).
        /// </summary>
        private static DateOnly DritterWerktag(DateOnly monat)
        {
            var tag = new DateOnly(monat.Year, monat.Month, 1);
            int werktage = 0;
            while (werktage < 3)
            {
                if (tag.DayOfWeek != DayOfWeek.Sunday)
                    werktage++;
                if (werktage < 3)
                    tag = tag.AddDays(1);
            }
            return tag;
        }

        private static void AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            satz.Buchungszeilen.Add(new Buchungszeile(sollHaben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = konto
            });
        }
    }
}
