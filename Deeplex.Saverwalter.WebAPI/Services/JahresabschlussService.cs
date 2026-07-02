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
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    /// <summary>
    /// Jahresabschlusskontrolle: Prüft pro Abrechnungszeitraum (Buchungsjahr), ob
    /// alle ausgleichbaren Konten ausgeglichen sind und alle Betriebskosten-
    /// abrechnungen erstellt und abgesendet wurden. Strikt lesend — anders als
    /// der OffeneForderungenController werden keine Sollstellungen erzeugt.
    /// </summary>
    public static class JahresabschlussService
    {
        /// <summary>Toleranz für Rundungsdifferenzen bei Geldbeträgen.</summary>
        private const decimal Epsilon = 0.005m;

        public class KontoJahresEntry
        {
            public int KontoId { get; set; }
            public string Kontonummer { get; set; } = "";
            public string Bezeichnung { get; set; } = "";
            public string Kontotyp { get; set; } = "";
            public string? Funktion { get; set; }
            public bool Ausgleichbar { get; set; }
            public string? VerknuepfungTyp { get; set; }
            public string? VerknuepfungId { get; set; }
            public string? VerknuepfungText { get; set; }
            /// <summary>Jahresverkehrszahlen: Zeilen mit Buchungsjahr == jahr.</summary>
            public decimal SollJahr { get; set; }
            public decimal HabenJahr { get; set; }
            /// <summary>Kumulierter Saldo aller Vorjahre (Soll-positiv).</summary>
            public decimal Saldovortrag { get; set; }
            /// <summary>Saldovortrag + SollJahr - HabenJahr.</summary>
            public decimal Endsaldo { get; set; }
            /// <summary>Ungedeckte Soll-Zeilen bis einschließlich jahr (OPOS-Sicht zum Jahresende).</summary>
            public int OffenePostenAnzahl { get; set; }
            public decimal OffenePostenBetrag { get; set; }
            /// <summary>Nicht ausgleichbare Konten gelten immer als ausgeglichen (Summenkonten).</summary>
            public bool Ausgeglichen { get; set; }
        }

        public class AbrechnungsstatusEntry
        {
            public int VertragId { get; set; }
            public string Bezeichnung { get; set; } = "";
            public bool ResultatVorhanden { get; set; }
            public bool Abgesendet { get; set; }
            public Guid? BuchungssatzId { get; set; }
        }

        public class JahresabschlussEntry
        {
            public int Jahr { get; set; }
            public List<KontoJahresEntry> Konten { get; set; } = [];
            public List<AbrechnungsstatusEntry> Abrechnungen { get; set; } = [];
            public int KontenOffen { get; set; }
            public int AbrechnungenGesamt { get; set; }
            public int AbrechnungenFertig { get; set; }
            public bool JahrAbgeschlossen { get; set; }
        }

        public class JahresUebersichtEntry
        {
            public int Jahr { get; set; }
            public int KontenOffen { get; set; }
            public int AbrechnungenOffen { get; set; }
            public bool Abgeschlossen { get; set; }
        }

        public static async Task<JahresabschlussEntry> ForJahrAsync(
            SaverwalterContext ctx, int jahr, HashSet<int>? scopedKontoIds)
        {
            var daten = await LadeDaten(ctx, scopedKontoIds);
            return BerechneJahr(daten, jahr);
        }

        /// <summary>
        /// Status aller Buchungsjahre (absteigend), in denen Buchungen der
        /// sichtbaren Konten existieren.
        /// </summary>
        public static async Task<List<JahresUebersichtEntry>> UebersichtAsync(
            SaverwalterContext ctx, HashSet<int>? scopedKontoIds)
        {
            var daten = await LadeDaten(ctx, scopedKontoIds);
            var jahre = daten.Konten
                .SelectMany(k => k.Buchungszeilen)
                .Select(z => z.Buchungssatz.Buchungsjahr)
                .Distinct()
                .OrderByDescending(j => j);

            return jahre
                .Select(jahr =>
                {
                    var abschluss = BerechneJahr(daten, jahr);
                    return new JahresUebersichtEntry
                    {
                        Jahr = jahr,
                        KontenOffen = abschluss.KontenOffen,
                        AbrechnungenOffen = abschluss.AbrechnungenGesamt - abschluss.AbrechnungenFertig,
                        Abgeschlossen = abschluss.JahrAbgeschlossen
                    };
                })
                .ToList();
        }

        private record Daten(
            List<Buchungskonto> Konten,
            List<KontoVerknuepfungEntry> Verknuepfungen,
            Dictionary<Guid, List<OffenerPostenAusgleich>> AusgleicheBySollZeile,
            List<Vertrag> Vertraege,
            List<Abrechnungsresultat> Resultate);

        private static async Task<Daten> LadeDaten(SaverwalterContext ctx, HashSet<int>? scopedKontoIds)
        {
            var kontenQuery = scopedKontoIds == null
                ? ctx.Buchungskonten
                : ctx.Buchungskonten.Where(k => scopedKontoIds.Contains(k.BuchungskontoId));
            var konten = await kontenQuery
                .Include(k => k.Buchungszeilen)
                    .ThenInclude(z => z.Buchungssatz)
                .OrderBy(k => k.Kontonummer)
                .ToListAsync();

            var kontoIds = konten.Select(k => k.BuchungskontoId).ToList();
            var verknuepfungen = await KontoVerknuepfungService.ForKontenAsync(ctx, kontoIds);

            var sollZeileIds = konten
                .SelectMany(k => k.Buchungszeilen)
                .Where(z => z.SollHaben == SollHaben.Soll)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();
            var ausgleicheBySollZeile = (await ctx.OffenePostenAusgleiche
                .Include(o => o.SollZeile)
                .Include(o => o.HabenZeile).ThenInclude(z => z.Buchungssatz)
                .Where(o => sollZeileIds.Contains(o.SollZeile.BuchungszeileId))
                .ToListAsync())
                .GroupBy(o => o.SollZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vertraege = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Mieter)
                .Include(v => v.MietBuchungskonto)
                .Where(v => scopedKontoIds == null
                    || scopedKontoIds.Contains(v.MietBuchungskonto.BuchungskontoId))
                .ToListAsync();

            var vertragIds = vertraege.Select(v => v.VertragId).ToHashSet();
            var resultate = (await ctx.Abrechnungsresultate
                .Include(r => r.Vertrag)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.StornoNach)
                .ToListAsync())
                .Where(r => vertragIds.Contains(r.Vertrag.VertragId))
                .ToList();

            return new Daten(konten, verknuepfungen, ausgleicheBySollZeile, vertraege, resultate);
        }

        private static JahresabschlussEntry BerechneJahr(Daten daten, int jahr)
        {
            var konten = daten.Konten
                .Where(k => k.Buchungszeilen.Any(z => z.Buchungssatz.Buchungsjahr <= jahr))
                .Select(k => BerechneKonto(daten, k, jahr))
                .OrderBy(k => k.Ausgleichbar ? 0 : 1)
                .ThenBy(k => k.Funktion)
                .ThenBy(k => k.Kontonummer)
                .ToList();

            var abrechnungen = BerechneAbrechnungen(daten, jahr);

            var kontenOffen = konten.Count(k => !k.Ausgeglichen);
            var abrechnungenFertig = abrechnungen.Count(a => a.Abgesendet);
            return new JahresabschlussEntry
            {
                Jahr = jahr,
                Konten = konten,
                Abrechnungen = abrechnungen,
                KontenOffen = kontenOffen,
                AbrechnungenGesamt = abrechnungen.Count,
                AbrechnungenFertig = abrechnungenFertig,
                JahrAbgeschlossen = kontenOffen == 0 && abrechnungenFertig == abrechnungen.Count
            };
        }

        private static KontoJahresEntry BerechneKonto(Daten daten, Buchungskonto konto, int jahr)
        {
            var verknuepfung = daten.Verknuepfungen
                .FirstOrDefault(v => v.KontoId == konto.BuchungskontoId);
            var ausgleichbar = verknuepfung?.Ausgleichbar ?? false;

            decimal Saldo(Func<Buchungszeile, bool> filter) => konto.Buchungszeilen
                .Where(filter)
                .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);

            var sollJahr = konto.Buchungszeilen
                .Where(z => z.Buchungssatz.Buchungsjahr == jahr && z.SollHaben == SollHaben.Soll)
                .Sum(z => z.Betrag);
            var habenJahr = konto.Buchungszeilen
                .Where(z => z.Buchungssatz.Buchungsjahr == jahr && z.SollHaben == SollHaben.Haben)
                .Sum(z => z.Betrag);
            var saldovortrag = Saldo(z => z.Buchungssatz.Buchungsjahr < jahr);
            var endsaldo = saldovortrag + sollJahr - habenJahr;

            // OPOS-Sicht zum Jahresende: Ausgleiche aus Folgejahren zählen noch nicht,
            // eine 2023 offene Forderung bleibt in der 2023er-Sicht offen, auch wenn
            // sie 2024 beglichen wurde.
            var offenePosten = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungssatz.Buchungsjahr <= jahr)
                .Select(z =>
                {
                    var gedeckt = daten.AusgleicheBySollZeile
                        .TryGetValue(z.BuchungszeileId, out var ausgleiche)
                        ? ausgleiche
                            .Where(a => a.HabenZeile.Buchungssatz.Buchungsjahr <= jahr)
                            .Sum(a => a.HabenZeile.Betrag)
                        : 0m;
                    return z.Betrag - gedeckt;
                })
                .Where(offen => offen > Epsilon)
                .ToList();

            return new KontoJahresEntry
            {
                KontoId = konto.BuchungskontoId,
                Kontonummer = konto.Kontonummer,
                Bezeichnung = konto.Bezeichnung,
                Kontotyp = konto.Kontotyp.ToString(),
                Funktion = verknuepfung?.Funktion,
                Ausgleichbar = ausgleichbar,
                VerknuepfungTyp = verknuepfung?.Typ,
                VerknuepfungId = verknuepfung?.Id,
                VerknuepfungText = verknuepfung?.Text,
                SollJahr = sollJahr,
                HabenJahr = habenJahr,
                Saldovortrag = saldovortrag,
                Endsaldo = endsaldo,
                OffenePostenAnzahl = offenePosten.Count,
                OffenePostenBetrag = offenePosten.Sum(),
                Ausgeglichen = !ausgleichbar
                    || (offenePosten.Count == 0 && Math.Abs(endsaldo) <= Epsilon)
            };
        }

        private static List<AbrechnungsstatusEntry> BerechneAbrechnungen(Daten daten, int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);

            return daten.Vertraege
                .Where(v => v.Versionen.Any(ver => ver.Beginn <= endOfYear)
                         && (v.Ende == null || v.Ende >= startOfYear))
                .Select(v =>
                {
                    // Stornierte Abrechnungssätze zählen nicht als vorhanden.
                    var resultat = daten.Resultate
                        .Where(r => r.Vertrag.VertragId == v.VertragId
                                 && r.Buchungssatz.Buchungsjahr == jahr
                                 && r.Buchungssatz.StornoNach == null)
                        .MaxBy(r => r.LastModified);
                    return new AbrechnungsstatusEntry
                    {
                        VertragId = v.VertragId,
                        Bezeichnung = KontoVerknuepfungService.GetVertragName(v),
                        ResultatVorhanden = resultat != null,
                        Abgesendet = resultat?.Abgesendet ?? false,
                        BuchungssatzId = resultat?.Buchungssatz.BuchungssatzId
                    };
                })
                .OrderBy(a => a.Bezeichnung)
                .ToList();
        }
    }
}
