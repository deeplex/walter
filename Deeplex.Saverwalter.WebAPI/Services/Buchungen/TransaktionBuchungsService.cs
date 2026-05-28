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

        public class GaragenmietInput
        {
            public int GarageVertragId { get; set; }
            public decimal Betrag { get; set; }
        }

        public class StandaloneGaragenmietInput
        {
            public int GarageVertragId { get; set; }
            public DateOnly BetreffenderMonat { get; set; }
            public decimal Betrag { get; set; }
        }

        public class MietzahlungsInput
        {
            public int VertragId { get; set; }
            public DateOnly BetreffenderMonat { get; set; }
            public decimal Kaltmiete { get; set; }
            public List<GaragenmietInput> Garagen { get; set; } = [];
            public decimal NkVorauszahlung { get; set; }
        }

        public class BetriebskostenEingangInput
        {
            public Guid? ExistingBuchungssatzId { get; set; }
            public int UmlageId { get; set; }
            public int BetreffendesJahr { get; set; }
            public DateOnly RechnungsDatum { get; set; }
            public decimal Betrag { get; set; }
            public string? Notiz { get; set; }
        }

        public class ErhaltungsaufwendungsInput
        {
            public Guid? ExistingBuchungssatzId { get; set; }
            public int WohnungId { get; set; }
            public decimal Betrag { get; set; }
            public string? Beschreibung { get; set; }
        }

        public class SonstigerBuchungssatzInput
        {
            public decimal Betrag { get; set; }
            public string? Beschreibung { get; set; }
        }

        public class NkAnteilEingangInput
        {
            public int VertragId { get; set; }
            public int UmlageId { get; set; }
            public int BetreffendesJahr { get; set; }
            public decimal Betrag { get; set; }
            public string? Notiz { get; set; }
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
            public List<StandaloneGaragenmietInput> GaragenEingaenge { get; set; } = [];
            public List<BetriebskostenEingangInput> BetriebskostenEingaenge { get; set; } = [];
            public List<ErhaltungsaufwendungsInput> Erhaltungsaufwendungen { get; set; } = [];
            public List<SonstigerBuchungssatzInput> Sonstige { get; set; } = [];
            public List<NkAnteilEingangInput> NkAnteilEingaenge { get; set; } = [];
        }

        // ── Hauptmethode ─────────────────────────────────────────────────────────

        public async Task<Transaktion> BucheAsync(TransaktionsInput input)
        {
            var totalBetrag =
                input.Mieten.Sum(m => m.Kaltmiete + m.NkVorauszahlung + m.Garagen.Sum(g => g.Betrag)) +
                input.GaragenEingaenge.Sum(g => g.Betrag) +
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

            if (transaktion.Zahler is null && input.BetriebskostenEingaenge.Count > 0)
                transaktion.Zahler = await ResolveZahlerFromBkAsync(input.BetriebskostenEingaenge[0]);

            if (input.Erhaltungsaufwendungen.Count > 0 && transaktion.Zahler is null)
                throw new ArgumentException("Erhaltungsaufwendungen erfordern ein Zahlerkonto.");

            if (input.Sonstige.Count > 0 && (transaktion.Zahler is null || transaktion.Zahlungsempfaenger is null))
                throw new ArgumentException("Sonstige Buchungssätze erfordern Zahler und Zahlungsempfänger.");

            _ctx.Transaktionen.Add(transaktion);

            foreach (var miete in input.Mieten)
                await ErstelleMietzahlungAsync(miete, transaktion);

            foreach (var garage in input.GaragenEingaenge)
                await ErstelleGaragenmietzahlungAsync(
                    new GaragenmietInput { GarageVertragId = garage.GarageVertragId, Betrag = garage.Betrag },
                    transaktion,
                    new DateOnly(garage.BetreffenderMonat.Year, garage.BetreffenderMonat.Month, 1));

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
                .Include(v => v.Mieter)
                .Include(v => v.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .FirstOrDefaultAsync(v => v.VertragId == miete.VertragId)
                ?? throw new ArgumentException($"Vertrag {miete.VertragId} nicht gefunden.");

            var monat = new DateOnly(miete.BetreffenderMonat.Year, miete.BetreffenderMonat.Month, 1);

            if (string.IsNullOrWhiteSpace(transaktion.Verwendungszweck))
            {
                var mieterNamen = vertrag.Mieter.Count > 0
                    ? string.Join(", ", vertrag.Mieter.Select(m => m.Bezeichnung))
                    : "Leerstand";
                var wohnungInfo = vertrag.Wohnung.Adresse?.Anschrift is { } a
                    ? $"{a} – {vertrag.Wohnung.Bezeichnung}"
                    : vertrag.Wohnung.Bezeichnung;
                transaktion.Verwendungszweck = $"Miete {monat:MM/yyyy} | {mieterNamen} | {wohnungInfo}";
            }

            if (miete.Kaltmiete > 0)
            {
                var version = vertrag.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn)
                    ?? throw new InvalidOperationException($"Keine VertragVersion für {monat:MM/yyyy}.");

                var sollZeile = FindeOderErstelleSollstellung(vertrag, monat, version);

                var schonGezahlt = sollZeile.AlsSollZeile.Sum(a => a.HabenZeile.Betrag);
                var verbleibend = sollZeile.Betrag - schonGezahlt;
                if (miete.Kaltmiete > verbleibend + 0.005m)
                    throw new InvalidOperationException(
                        $"Kaltmiete ({miete.Kaltmiete:C}) übersteigt verbleibende Forderung ({verbleibend:C}).");

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

            foreach (var garage in miete.Garagen)
                await ErstelleGaragenmietzahlungAsync(garage, transaktion, monat);

            if (miete.NkVorauszahlung > 0)
            {
                var nkSatz = new Buchungssatz(transaktion.Zahlungsdatum, $"Mietzahlung NK-VZ {monat:MM/yyyy}");
                AddZeile(nkSatz, SollHaben.Soll, miete.NkVorauszahlung, vertrag.ZahlungsKonto);
                AddZeile(nkSatz, SollHaben.Haben, miete.NkVorauszahlung, vertrag.NkBuchungskonto);
                nkSatz.Transaktion = transaktion;
                _ctx.Buchungssaetze.Add(nkSatz);
            }
        }

        private async Task ErstelleGaragenmietzahlungAsync(GaragenmietInput garage, Transaktion transaktion, DateOnly monat)
        {
            var gv = await _ctx.GarageVertraege
                .Include(g => g.Versionen)
                .Include(g => g.Garage).ThenInclude(g => g.Ertragskonto)
                .Include(g => g.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(g => g.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile)
                .Include(g => g.ZahlungsKonto)
                .FirstOrDefaultAsync(g => g.GarageVertragId == garage.GarageVertragId)
                ?? throw new ArgumentException($"GarageVertrag {garage.GarageVertragId} nicht gefunden.");

            var version = gv.Versionen.Where(v => v.Beginn <= monat).MaxBy(v => v.Beginn)
                ?? throw new InvalidOperationException($"Keine GarageVertragVersion für {monat:MM/yyyy}.");

            var sollZeile = FindeOderErstelleGarageSollstellung(gv, monat, version);

            var schonGezahlt = sollZeile.AlsSollZeile.Sum(a => a.HabenZeile.Betrag);
            var verbleibend = sollZeile.Betrag - schonGezahlt;
            if (garage.Betrag > verbleibend + 0.005m)
                throw new InvalidOperationException(
                    $"Garagenmiete ({garage.Betrag:C}) übersteigt verbleibende Forderung ({verbleibend:C}).");

            var satz = new Buchungssatz(transaktion.Zahlungsdatum, $"Garagenmietzahlung {monat:MM/yyyy} | {gv.Garage.Kennung}");
            AddZeile(satz, SollHaben.Soll, garage.Betrag, gv.ZahlungsKonto);
            AddZeile(satz, SollHaben.Haben, garage.Betrag, gv.MietBuchungskonto);
            satz.Transaktion = transaktion;
            _ctx.Buchungssaetze.Add(satz);

            var habenZeile = satz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben);
            _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = sollZeile,
                HabenZeile = habenZeile
            });
        }

        private Buchungszeile FindeOderErstelleGarageSollstellung(GarageVertrag gv, DateOnly monat, GarageVertragVersion version)
        {
            if (gv.Ende.HasValue && monat > new DateOnly(gv.Ende.Value.Year, gv.Ende.Value.Month, 1))
                throw new InvalidOperationException(
                    $"GarageVertrag {gv.GarageVertragId} endete am {gv.Ende.Value:dd.MM.yyyy}. Keine Sollstellung für {monat:MM/yyyy} möglich.");

            var vorhandene = gv.MietBuchungskonto.Buchungszeilen
                .FirstOrDefault(z =>
                    z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                    && z.Buchungssatz.Buchungsdatum.Month == monat.Month);

            if (vorhandene != null) return vorhandene;

            var satz = new Buchungssatz(DritterWerktag(monat), $"Garagenmietsoll {monat:MM/yyyy} | {gv.Garage.Kennung}");
            AddZeile(satz, SollHaben.Soll, version.GaragenMiete, gv.MietBuchungskonto);
            AddZeile(satz, SollHaben.Haben, version.GaragenMiete, gv.Garage.Ertragskonto);
            _ctx.Buchungssaetze.Add(satz);
            return satz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll);
        }

        /// <summary>
        /// Erstellt atomisch:
        ///   1. Forderungs-Buchungssatz: Haben Umlage.NkVerrechnungsKonto (Soll kommt mit der Jahresabrechnung)
        ///   2. Zahlungs-Buchungssatz (→ Transaktion): Soll NkVerrechnungsKonto / Haben ZahlungsKonto
        /// </summary>
        private async Task<Buchungssatz> ErstelleBetriebskostenEingangAsync(
            BetriebskostenEingangInput bk, DateOnly zahlungsdatum)
        {
            if (bk.ExistingBuchungssatzId.HasValue)
            {
                var forderungsSatz = await _ctx.Buchungssaetze
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile)
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                    .FirstOrDefaultAsync(s => s.BuchungssatzId == bk.ExistingBuchungssatzId.Value)
                    ?? throw new ArgumentException($"Buchungssatz {bk.ExistingBuchungssatzId} nicht gefunden.");

                var forderungsHabenZeile = forderungsSatz.Buchungszeilen
                    .First(z => z.SollHaben == SollHaben.Haben);
                var schonGezahlt = forderungsHabenZeile.AlsHabenZeile.Sum(a => a.SollZeile.Betrag);
                var verbleibend = forderungsHabenZeile.Betrag - schonGezahlt;
                if (bk.Betrag > verbleibend + 0.005m)
                    throw new InvalidOperationException(
                        $"Zahlungsbetrag ({bk.Betrag:C}) übersteigt verbleibende Forderung ({verbleibend:C}).");

                var nkVk = forderungsHabenZeile.Buchungskonto;
                var umlageForZahlung = await _ctx.Umlagen
                    .Include(u => u.ZahlungsKonto)
                    .Include(u => u.Typ)
                    .FirstOrDefaultAsync(u => u.NkVerrechnungsKonto.BuchungskontoId == nkVk.BuchungskontoId)
                    ?? throw new InvalidOperationException("Umlage für Buchungssatz nicht gefunden.");

                var zahlungsSatz = new Buchungssatz(
                    zahlungsdatum,
                    $"Zahlung Betriebskosten {umlageForZahlung.Typ.Bezeichnung} {forderungsSatz.Buchungsjahr}");
                AddZeile(zahlungsSatz, SollHaben.Soll, bk.Betrag, nkVk);
                AddZeile(zahlungsSatz, SollHaben.Haben, bk.Betrag, umlageForZahlung.ZahlungsKonto);

                _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = zahlungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
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

            var neuerForderungsSatz = new Buchungssatz(
                bk.RechnungsDatum,
                $"Betriebskosten {umlage.Typ.Bezeichnung} {bk.BetreffendesJahr}")
            {
                Buchungsjahr = bk.BetreffendesJahr,
                Notiz = bk.Notiz
            };
            AddZeile(neuerForderungsSatz, SollHaben.Haben, bk.Betrag, umlage.NkVerrechnungsKonto);
            _ctx.Buchungssaetze.Add(neuerForderungsSatz);

            var neuerZahlungsSatz = new Buchungssatz(
                zahlungsdatum,
                $"Zahlung Betriebskosten {umlage.Typ.Bezeichnung} {bk.BetreffendesJahr}");
            AddZeile(neuerZahlungsSatz, SollHaben.Soll, bk.Betrag, umlage.NkVerrechnungsKonto);
            AddZeile(neuerZahlungsSatz, SollHaben.Haben, bk.Betrag, umlage.ZahlungsKonto);

            _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = neuerZahlungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
                HabenZeile = neuerForderungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben)
            });

            return neuerZahlungsSatz;
        }

        private async Task<Buchungssatz> ErstelleErhaltungsaufwendungAsync(
            ErhaltungsaufwendungsInput ea, DateOnly zahlungsdatum, Transaktion transaktion)
        {
            if (ea.ExistingBuchungssatzId.HasValue)
            {
                var existingSatz = await _ctx.Buchungssaetze
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile)
                    .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                    .FirstOrDefaultAsync(s => s.BuchungssatzId == ea.ExistingBuchungssatzId.Value)
                    ?? throw new ArgumentException($"Buchungssatz {ea.ExistingBuchungssatzId} nicht gefunden.");

                var habenZeile = existingSatz.Buchungszeilen
                    .First(z => z.SollHaben == SollHaben.Haben);
                var schonGezahlt = habenZeile.AlsHabenZeile.Sum(a => a.SollZeile.Betrag);
                var verbleibend = habenZeile.Betrag - schonGezahlt;
                if (ea.Betrag > verbleibend + 0.005m)
                    throw new InvalidOperationException(
                        $"Zahlungsbetrag ({ea.Betrag:C}) übersteigt verbleibende Verbindlichkeit ({verbleibend:C}).");

                var verbindlichkeitsKonto = habenZeile.Buchungskonto;
                var zahlungsSatz = new Buchungssatz(zahlungsdatum,
                    ea.Beschreibung is { Length: > 0 } b
                        ? $"Zahlung Erhaltungsaufwendung {b}"
                        : $"Zahlung {existingSatz.Beschreibung}");
                AddZeile(zahlungsSatz, SollHaben.Soll, ea.Betrag, verbindlichkeitsKonto);
                AddZeile(zahlungsSatz, SollHaben.Haben, ea.Betrag, transaktion.Zahler!.BuchungsKonto);

                _ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = zahlungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
                    HabenZeile = habenZeile
                });

                return zahlungsSatz;
            }

            var wohnung = await _ctx.Wohnungen
                .Include(w => w.AufwandsKonto)
                .FirstOrDefaultAsync(w => w.WohnungId == ea.WohnungId)
                ?? throw new ArgumentException($"Wohnung {ea.WohnungId} nicht gefunden.");

            var beschreibung = ea.Beschreibung is { Length: > 0 } desc
                ? $"Erhaltungsaufwendung {desc}"
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

        private async Task<Bankkonto?> ResolveZahlerFromBkAsync(BetriebskostenEingangInput bk)
        {
            var umlageId = bk.ExistingBuchungssatzId.HasValue
                ? await _ctx.Umlagen
                    .Where(u => u.NkVerrechnungsKonto.Buchungszeilen.Any(z =>
                        z.Buchungssatz.BuchungssatzId == bk.ExistingBuchungssatzId.Value
                        && z.SollHaben == SollHaben.Haben))
                    .Select(u => (int?)u.UmlageId)
                    .FirstOrDefaultAsync()
                : bk.UmlageId;

            if (umlageId is null) return null;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var besitzerIds = await _ctx.Umlagen
                .Where(u => u.UmlageId == umlageId)
                .SelectMany(u => u.Wohnungen)
                .SelectMany(w => w.Eigentuemer)
                .Where(e => e.Bis == null || e.Bis >= today)
                .Select(e => e.Kontakt.KontaktId)
                .Distinct()
                .ToListAsync();

            if (besitzerIds.Count == 0) return null;

            return await _ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .FirstOrDefaultAsync(b => b.Besitzer.Any(k => besitzerIds.Contains(k.KontaktId)));
        }

        private Buchungszeile FindeOderErstelleSollstellung(Vertrag vertrag, DateOnly monat, VertragVersion version)
        {
            if (vertrag.Ende.HasValue && monat > new DateOnly(vertrag.Ende.Value.Year, vertrag.Ende.Value.Month, 1))
                throw new InvalidOperationException(
                    $"Vertrag {vertrag.VertragId} endete am {vertrag.Ende.Value:dd.MM.yyyy}. Keine Sollstellung für {monat:MM/yyyy} möglich.");

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

        private static DateOnly DritterWerktag(DateOnly monat) => DateUtils.DritterWerktag(monat);

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
