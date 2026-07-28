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
    /// Korrigiert bereits bezahlte Miet-Monate, wenn eine <see cref="VertragVersion"/>
    /// rückwirkend mit einer anderen Grundmiete angelegt wurde (z. B. eine Mieterhöhung,
    /// die erst nachträglich im System erfasst wurde). Rein additiv: kein bestehender
    /// Buchungssatz (Sollstellung, Kaltmiete-Zahlung, NK-VZ-Zahlung) wird jemals mutiert
    /// oder storniert.
    ///
    /// Für einen betroffenen, bereits bezahlten Monat werden zwei neue Buchungssätze und
    /// ein neuer OPOS-Ausgleich angelegt, deren Soll-/Haben-Richtung vom Vorzeichen des
    /// Deltas (neue Grundmiete − gebuchte Kaltmiete) abhängt:
    ///
    ///   delta &gt; 0 (Miete erhöht sich):
    ///     1. Zusatz-Forderung: Soll MietBuchungskonto(delta) / Haben MietErtragskonto(delta)
    ///     2. Umbuchung:        Soll NkBuchungskonto(delta)   / Haben MietBuchungskonto(delta)
    ///     3. OPOS: SollZeile = Zusatz-Forderung, HabenZeile = Umbuchung
    ///
    ///   delta &lt; 0 (Miete sinkt rückwirkend), mit d = |delta|:
    ///     1. Gutschrift:       Haben MietBuchungskonto(d) / Soll MietErtragskonto(d)
    ///     2. Umbuchung:        Haben NkBuchungskonto(d)   / Soll MietBuchungskonto(d)
    ///     3. OPOS: SollZeile = Umbuchung, HabenZeile = Gutschrift
    ///
    /// In beiden Fällen liegen SollZeile und HabenZeile des neuen OPOS-Eintrags auf
    /// demselben Konto (MietBuchungskonto) — die OPOS-Invariante bleibt gewahrt — und das
    /// Mietforderungskonto ist für den Monat danach wieder voll ausgeglichen.
    ///
    /// Unbezahlte Monate (nur Sollstellung, kein OPOS) sind hier bewusst ausgeschlossen:
    /// die Sollstellung ist dort noch frei (<see cref="BuchungssatzSchutz.KannLoeschen"/>)
    /// und lässt sich direkt über den bestehenden Buchungssatz-Bearbeiten-Endpunkt
    /// korrigieren.
    ///
    /// Idempotenz: eine bereits angewandte Korrektur für einen Monat wird über den
    /// eigenen Umbuchungssatz wiedergefunden — identifiziert über das Konto-Paar
    /// (MietBuchungskonto/NkBuchungskonto) und den im Beschreibungstext eingebetteten
    /// Zielmonat (analog zu <c>AbrechnungslaufService.EnsureStrompauschaleUmbuchungAsync</c>,
    /// das Umbuchungen über Konto-Paar + Jahr wiederfindet).
    /// </summary>
    public class MietKorrekturService(SaverwalterContext ctx)
    {
        public enum MietKorrekturStatus
        {
            Anwendbar,
            Abgerechnet,
            NkReichtNichtAus,
            BereitsKorrigiert,
        }

        public class MietKorrekturMonat
        {
            public int Jahr { get; set; }
            public int Monat { get; set; }
            public decimal IstKaltmiete { get; set; }
            public decimal SollKaltmiete { get; set; }
            public decimal Delta => SollKaltmiete - IstKaltmiete;
            public MietKorrekturStatus Status { get; set; }
        }

        public class MietKorrekturErgebnis
        {
            public List<(int Jahr, int Monat)> Korrigiert { get; set; } = [];
            public List<(int Jahr, int Monat, string Grund)> Uebersprungen { get; set; } = [];
        }

        private class MonatsDaten
        {
            public int Jahr { get; set; }
            public int Monat { get; set; }
            public decimal SollstellungsBetrag { get; set; }
            public required Buchungszeile? NkZahlungHabenZeile { get; set; }
        }

        public async Task<IReadOnlyList<MietKorrekturMonat>> ErmittleAsync(int vertragId)
        {
            var vertrag = await LadeVertragAsync(vertragId);
            return await ErmittleAsync(vertrag);
        }

        private async Task<IReadOnlyList<MietKorrekturMonat>> ErmittleAsync(Vertrag vertrag)
        {
            var monate = await LadeBezahlteMonateAsync(vertrag);
            var result = new List<MietKorrekturMonat>();

            foreach (var m in monate)
            {
                var version = vertrag.Versionen
                    .Where(v => v.Beginn <= new DateOnly(m.Jahr, m.Monat, 1))
                    .MaxBy(v => v.Beginn);
                if (version is null) continue;

                var delta = version.Grundmiete - m.SollstellungsBetrag;
                if (Math.Abs(delta) <= 0.005m) continue;

                var status = await BestimmeStatusAsync(vertrag, m, delta);
                result.Add(new MietKorrekturMonat
                {
                    Jahr = m.Jahr,
                    Monat = m.Monat,
                    IstKaltmiete = m.SollstellungsBetrag,
                    SollKaltmiete = version.Grundmiete,
                    Status = status
                });
            }

            return result;
        }

        public async Task<MietKorrekturErgebnis> AnwendenAsync(
            int vertragId, IReadOnlySet<(int Jahr, int Monat)> monate, string grund)
        {
            if (string.IsNullOrWhiteSpace(grund))
                throw new ArgumentException("Für eine Miet-Korrektur muss ein Grund angegeben werden.");

            var vertrag = await LadeVertragAsync(vertragId);
            var alle = await ErmittleAsync(vertrag);

            var ergebnis = new MietKorrekturErgebnis();
            foreach (var monat in alle.Where(m => monate.Contains((m.Jahr, m.Monat))))
            {
                if (monat.Status != MietKorrekturStatus.Anwendbar)
                {
                    ergebnis.Uebersprungen.Add((monat.Jahr, monat.Monat, monat.Status.ToString()));
                    continue;
                }

                BucheKorrektur(vertrag, monat, grund);
                ergebnis.Korrigiert.Add((monat.Jahr, monat.Monat));
            }

            await ctx.SaveChangesAsync();
            return ergebnis;
        }

        private void BucheKorrektur(Vertrag vertrag, MietKorrekturMonat monat, string grund)
        {
            var monatsDatum = new DateOnly(monat.Jahr, monat.Monat, 1);
            var heute = DateOnly.FromDateTime(DateTime.Today);

            if (monat.Delta > 0)
            {
                var delta = monat.Delta;

                var forderungsSatz = new Buchungssatz(heute,
                    $"Umbuchung Mietsoll-Nachtrag {monatsDatum:MM/yyyy} (rückwirkende Erhöhung, {grund})");
                AddZeile(forderungsSatz, SollHaben.Soll, delta, vertrag.MietBuchungskonto);
                AddZeile(forderungsSatz, SollHaben.Haben, delta, vertrag.Wohnung.MietErtragskonto);
                ctx.Buchungssaetze.Add(forderungsSatz);

                var umbuchungsSatz = new Buchungssatz(heute,
                    $"Umbuchung NK-Vorauszahlung → Kaltmiete {monatsDatum:MM/yyyy} ({grund})");
                AddZeile(umbuchungsSatz, SollHaben.Soll, delta, vertrag.NkBuchungskonto);
                AddZeile(umbuchungsSatz, SollHaben.Haben, delta, vertrag.MietBuchungskonto);
                ctx.Buchungssaetze.Add(umbuchungsSatz);

                ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = forderungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
                    HabenZeile = umbuchungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben)
                });
            }
            else
            {
                var d = -monat.Delta;

                var gutschriftSatz = new Buchungssatz(heute,
                    $"Umbuchung Mietsoll-Gutschrift {monatsDatum:MM/yyyy} (rückwirkende Reduzierung, {grund})");
                AddZeile(gutschriftSatz, SollHaben.Haben, d, vertrag.MietBuchungskonto);
                AddZeile(gutschriftSatz, SollHaben.Soll, d, vertrag.Wohnung.MietErtragskonto);
                ctx.Buchungssaetze.Add(gutschriftSatz);

                var umbuchungsSatz = new Buchungssatz(heute,
                    $"Umbuchung Kaltmiete → NK-Vorauszahlung {monatsDatum:MM/yyyy} ({grund})");
                AddZeile(umbuchungsSatz, SollHaben.Haben, d, vertrag.NkBuchungskonto);
                AddZeile(umbuchungsSatz, SollHaben.Soll, d, vertrag.MietBuchungskonto);
                ctx.Buchungssaetze.Add(umbuchungsSatz);

                ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = umbuchungsSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll),
                    HabenZeile = gutschriftSatz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Haben)
                });
            }
        }

        private async Task<MietKorrekturStatus> BestimmeStatusAsync(Vertrag vertrag, MonatsDaten m, decimal delta)
        {
            var monatsToken = new DateOnly(m.Jahr, m.Monat, 1).ToString("MM/yyyy");
            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;

            var bereitsKorrigiert = await ctx.Buchungssaetze
                .Where(s => s.Beschreibung.StartsWith("Umbuchung") && s.Beschreibung.Contains(monatsToken))
                .Where(s => s.StornoNach == null)
                .Where(s =>
                    s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == mietKontoId) &&
                    s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == nkKontoId))
                .AnyAsync();
            if (bereitsKorrigiert) return MietKorrekturStatus.BereitsKorrigiert;

            var abgerechnet = await ctx.Abrechnungsresultate
                .AnyAsync(r => r.Vertrag.VertragId == vertrag.VertragId && r.Buchungssatz.Buchungsjahr == m.Jahr);
            if (abgerechnet) return MietKorrekturStatus.Abgerechnet;

            if (delta > 0)
            {
                var nkBetrag = m.NkZahlungHabenZeile?.Betrag ?? 0m;
                if (nkBetrag < delta - 0.005m) return MietKorrekturStatus.NkReichtNichtAus;
            }

            return MietKorrekturStatus.Anwendbar;
        }

        private async Task<Vertrag> LadeVertragAsync(int vertragId) =>
            await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.Wohnung).ThenInclude(w => w.MietErtragskonto)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId)
                ?? throw new ArgumentException($"Vertrag {vertragId} nicht gefunden.");

        /// <summary>
        /// Findet je Monat die ursprüngliche Sollstellung (nicht transaktionsgebunden) mit
        /// einer bereits erfolgten Kaltmiete-Zahlung (transaktionsgebundene OPOS-Gegenzeile).
        /// Unbezahlte Monate tauchen hier bewusst nicht auf (siehe Klassendokumentation).
        /// </summary>
        private async Task<List<MonatsDaten>> LadeBezahlteMonateAsync(Vertrag vertrag)
        {
            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;

            var sollZeilen = await ctx.Buchungszeilen
                .Where(z =>
                    z.Buchungskonto.BuchungskontoId == mietKontoId &&
                    z.SollHaben == SollHaben.Soll &&
                    z.Buchungssatz.Transaktion == null)
                .Include(z => z.Buchungssatz)
                .Include(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion)
                .ToListAsync();

            var monate = new List<MonatsDaten>();
            foreach (var gruppe in sollZeilen.GroupBy(z => new
            { z.Buchungssatz.Buchungsjahr, z.Buchungssatz.Buchungsdatum.Month }))
            {
                var sollZeile = gruppe.OrderBy(z => z.Buchungssatz.Buchungsdatum).First();

                var kaltmieteHabenZeile = sollZeile.AlsSollZeile
                    .Select(a => a.HabenZeile)
                    .FirstOrDefault(h => h.Buchungssatz.Transaktion != null);
                if (kaltmieteHabenZeile is null) continue; // unbezahlt: anderer Weg (siehe oben)

                var transaktionId = kaltmieteHabenZeile.Buchungssatz.Transaktion!.TransaktionId;
                var nkHabenZeile = await ctx.Buchungszeilen
                    .Where(z =>
                        z.Buchungskonto.BuchungskontoId == nkKontoId &&
                        z.SollHaben == SollHaben.Haben &&
                        z.Buchungssatz.Transaktion != null &&
                        z.Buchungssatz.Transaktion.TransaktionId == transaktionId &&
                        z.Buchungssatz.Buchungsdatum == kaltmieteHabenZeile.Buchungssatz.Buchungsdatum)
                    .FirstOrDefaultAsync();

                monate.Add(new MonatsDaten
                {
                    Jahr = gruppe.Key.Buchungsjahr,
                    Monat = gruppe.Key.Month,
                    SollstellungsBetrag = sollZeile.Betrag,
                    NkZahlungHabenZeile = nkHabenZeile
                });
            }

            return monate;
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
