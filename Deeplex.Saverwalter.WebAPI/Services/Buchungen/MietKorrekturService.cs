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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
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
    /// Alle Sätze einer Korrektur verweisen über <see cref="Buchungssatz.KorrigiertSollstellung"/>
    /// auf die Sollstellung des korrigierten Monats. Ein Monat gilt als korrigiert, solange
    /// einer dieser Sätze nicht storniert ist.
    ///
    /// Eine Erhöhung wird aus den NK-Vorauszahlungen des Jahres gedeckt (siehe
    /// <see cref="NkVorauszahlung"/>). Mehrere Monate eines Jahres teilen sich diesen Betrag.
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
            public required Buchungssatz Sollstellung { get; set; }
            public decimal SollstellungsBetrag { get; set; }
        }

        private record Kandidat(MietKorrekturMonat Monat, Buchungssatz Sollstellung);

        public async Task<IReadOnlyList<MietKorrekturMonat>> ErmittleAsync(int vertragId)
        {
            var vertrag = await LadeVertragAsync(vertragId);
            var nkVorauszahlungen = await LadeNkVorauszahlungenAsync(vertrag);
            var kandidaten = await ErmittleKandidatenAsync(vertrag, nkVorauszahlungen);
            return kandidaten.Select(k => k.Monat).ToList();
        }

        private async Task<List<Kandidat>> ErmittleKandidatenAsync(
            Vertrag vertrag, IReadOnlyDictionary<int, decimal> nkVorauszahlungen)
        {
            var monate = await LadeBezahlteMonateAsync(vertrag);
            var sollstellungIds = monate.Select(m => m.Sollstellung.BuchungssatzId).ToList();
            var korrigiert = (await ctx.Buchungssaetze
                .Where(s => s.KorrigiertSollstellung != null
                    && sollstellungIds.Contains(s.KorrigiertSollstellung.BuchungssatzId)
                    && s.StornoNach == null)
                .Select(s => s.KorrigiertSollstellung!.BuchungssatzId)
                .ToListAsync())
                .ToHashSet();
            var abgerechnet = await AbrechnungsschutzService.AbgerechneteJahreVertrag(ctx, vertrag.VertragId);

            var result = new List<Kandidat>();
            foreach (var m in monate)
            {
                var version = vertrag.Versionen
                    .Where(v => v.Beginn <= new DateOnly(m.Jahr, m.Monat, 1))
                    .MaxBy(v => v.Beginn);
                if (version is null) continue;

                var delta = version.Grundmiete - m.SollstellungsBetrag;
                if (Math.Abs(delta) <= 0.005m) continue;

                var status =
                    korrigiert.Contains(m.Sollstellung.BuchungssatzId) ? MietKorrekturStatus.BereitsKorrigiert
                    : abgerechnet.Contains(m.Jahr) ? MietKorrekturStatus.Abgerechnet
                    : !NkReichtAus(nkVorauszahlungen.GetValueOrDefault(m.Jahr), delta) ? MietKorrekturStatus.NkReichtNichtAus
                    : MietKorrekturStatus.Anwendbar;

                result.Add(new Kandidat(new MietKorrekturMonat
                {
                    Jahr = m.Jahr,
                    Monat = m.Monat,
                    IstKaltmiete = m.SollstellungsBetrag,
                    SollKaltmiete = version.Grundmiete,
                    Status = status
                }, m.Sollstellung));
            }

            return result;
        }

        private static bool NkReichtAus(decimal nkVorauszahlung, decimal delta) =>
            delta <= 0 || nkVorauszahlung >= delta - 0.005m;

        public async Task<MietKorrekturErgebnis> AnwendenAsync(
            int vertragId, IReadOnlySet<(int Jahr, int Monat)> monate, string grund)
        {
            if (string.IsNullOrWhiteSpace(grund))
                throw new ArgumentException("Für eine Miet-Korrektur muss ein Grund angegeben werden.");

            var vertrag = await LadeVertragAsync(vertragId);
            var nkVorauszahlungen = await LadeNkVorauszahlungenAsync(vertrag);
            var kandidaten = await ErmittleKandidatenAsync(vertrag, nkVorauszahlungen);
            var verbleibend = new Dictionary<int, decimal>(nkVorauszahlungen);

            var ergebnis = new MietKorrekturErgebnis();
            foreach (var (monat, sollstellung) in kandidaten
                .Where(k => monate.Contains((k.Monat.Jahr, k.Monat.Monat)))
                .OrderBy(k => k.Monat.Jahr).ThenBy(k => k.Monat.Monat))
            {
                var status = monat.Status == MietKorrekturStatus.Anwendbar
                    && !NkReichtAus(verbleibend.GetValueOrDefault(monat.Jahr), monat.Delta)
                    ? MietKorrekturStatus.NkReichtNichtAus
                    : monat.Status;
                if (status != MietKorrekturStatus.Anwendbar)
                {
                    ergebnis.Uebersprungen.Add((monat.Jahr, monat.Monat, status.ToString()));
                    continue;
                }

                BucheKorrektur(vertrag, monat, sollstellung, grund);
                verbleibend[monat.Jahr] = verbleibend.GetValueOrDefault(monat.Jahr) - monat.Delta;
                ergebnis.Korrigiert.Add((monat.Jahr, monat.Monat));
            }

            await ctx.SaveChangesAsync();
            return ergebnis;
        }

        private void BucheKorrektur(Vertrag vertrag, MietKorrekturMonat monat, Buchungssatz sollstellung, string grund)
        {
            var monatsDatum = new DateOnly(monat.Jahr, monat.Monat, 1);
            var heute = DateOnly.FromDateTime(DateTime.Today);
            // Buchungsdatum ist der Tag der Korrektur, Buchungsjahr das Jahr des korrigierten Monats.
            Buchungssatz NeuerSatz(string beschreibung) =>
                new(heute, beschreibung) { Buchungsjahr = monat.Jahr, KorrigiertSollstellung = sollstellung };

            if (monat.Delta > 0)
            {
                var delta = monat.Delta;

                var forderungsSatz = NeuerSatz(
                    $"Umbuchung Mietsoll-Nachtrag {monatsDatum:MM/yyyy} (rückwirkende Erhöhung, {grund})");
                AddZeile(forderungsSatz, SollHaben.Soll, delta, vertrag.MietBuchungskonto);
                AddZeile(forderungsSatz, SollHaben.Haben, delta, vertrag.Wohnung.MietErtragskonto);
                ctx.Buchungssaetze.Add(forderungsSatz);

                var umbuchungsSatz = NeuerSatz(
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

                var gutschriftSatz = NeuerSatz(
                    $"Umbuchung Mietsoll-Gutschrift {monatsDatum:MM/yyyy} (rückwirkende Reduzierung, {grund})");
                AddZeile(gutschriftSatz, SollHaben.Haben, d, vertrag.MietBuchungskonto);
                AddZeile(gutschriftSatz, SollHaben.Soll, d, vertrag.Wohnung.MietErtragskonto);
                ctx.Buchungssaetze.Add(gutschriftSatz);

                var umbuchungsSatz = NeuerSatz(
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

        /// <summary>NK-Vorauszahlungen je Buchungsjahr, berechnet wie in der Abrechnung.</summary>
        private async Task<Dictionary<int, decimal>> LadeNkVorauszahlungenAsync(Vertrag vertrag)
        {
            var zeilen = await ctx.Buchungszeilen
                .Where(NkVorauszahlung.Zaehlt(vertrag))
                .Select(z => new { z.Buchungssatz.Buchungsjahr, z.SollHaben, z.Betrag })
                .ToListAsync();

            return zeilen
                .GroupBy(z => z.Buchungsjahr)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag));
        }

        private async Task<Vertrag> LadeVertragAsync(int vertragId) =>
            await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
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

                monate.Add(new MonatsDaten
                {
                    Jahr = gruppe.Key.Buchungsjahr,
                    Monat = gruppe.Key.Month,
                    Sollstellung = sollZeile.Buchungssatz,
                    SollstellungsBetrag = sollZeile.Betrag
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
