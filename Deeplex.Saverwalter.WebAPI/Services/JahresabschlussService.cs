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
            /// <summary>Ungedeckte Zeilen mit Buchungsjahr == jahr.</summary>
            public int OffenePostenAnzahl { get; set; }
            public decimal OffenePostenBetrag { get; set; }
            /// <summary>Nicht ausgleichbare Konten gelten immer als ausgeglichen (Summenkonten).</summary>
            public bool Ausgeglichen { get; set; }
            /// <summary>
            /// Konto eines Vertrags, für den in diesem Jahr ein Abrechnungsverzicht
            /// hinterlegt ist — offene Forderungen zählen dann nicht als offener Punkt.
            /// </summary>
            public bool Verzichtet { get; set; }
        }

        public class AbrechnungsstatusEntry
        {
            public int VertragId { get; set; }
            public string Bezeichnung { get; set; } = "";
            public bool ResultatVorhanden { get; set; }
            public bool Abgesendet { get; set; }
            /// <summary>Saldo per OPOS gedeckt (oder 0). Nur aussagekräftig wenn ResultatVorhanden.</summary>
            public bool Ausgeglichen { get; set; }
            public Guid? BuchungssatzId { get; set; }
            /// <summary>Dokumentierter Abrechnungsverzicht — gilt als erledigt, ohne Buchung.</summary>
            public bool Verzichtet { get; set; }
            public string? VerzichtGrund { get; set; }
            public Guid? VerzichtId { get; set; }
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
            Dictionary<Guid, List<OffenerPostenAusgleich>> AusgleicheByHabenZeile,
            List<Vertrag> Vertraege,
            List<Abrechnungsresultat> Resultate,
            List<Abrechnungsverzicht> Verzichte);

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

            // OPOS-Ausgleiche für beide Richtungen: eine Soll-Forderung (z.B. Miete,
            // Nachzahlung) wird von einer Haben-Zeile gedeckt, eine Haben-Forderung
            // (z.B. Guthaben) von einer Soll-Zeile. Bewusst OHNE Jahresfilter — ein
            // Ausgleich zählt unabhängig davon, in welchem Jahr er gebucht wurde.
            var alleZeileIds = konten
                .SelectMany(k => k.Buchungszeilen)
                .Select(z => z.BuchungszeileId)
                .ToHashSet();
            var alleAusgleiche = await ctx.OffenePostenAusgleiche
                .Include(o => o.SollZeile).ThenInclude(z => z.Buchungssatz)
                .Include(o => o.HabenZeile).ThenInclude(z => z.Buchungssatz)
                .Where(o => alleZeileIds.Contains(o.SollZeile.BuchungszeileId)
                         || alleZeileIds.Contains(o.HabenZeile.BuchungszeileId))
                .ToListAsync();
            var ausgleicheBySollZeile = alleAusgleiche
                .GroupBy(o => o.SollZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var ausgleicheByHabenZeile = alleAusgleiche
                .GroupBy(o => o.HabenZeile.BuchungszeileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vertraege = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Mieter)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.BkAbrechnungsKonto)
                .Where(v => scopedKontoIds == null
                    || scopedKontoIds.Contains(v.MietBuchungskonto.BuchungskontoId))
                .ToListAsync();

            var vertragIds = vertraege.Select(v => v.VertragId).ToHashSet();
            var resultate = (await ctx.Abrechnungsresultate
                .Include(r => r.Vertrag)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile)
                .ToListAsync())
                .Where(r => vertragIds.Contains(r.Vertrag.VertragId))
                .ToList();

            var verzichte = (await ctx.Abrechnungsverzichte
                .Include(v => v.Vertrag)
                .ToListAsync())
                .Where(v => vertragIds.Contains(v.Vertrag.VertragId))
                .ToList();

            return new Daten(konten, verknuepfungen, ausgleicheBySollZeile, ausgleicheByHabenZeile, vertraege, resultate, verzichte);
        }

        private static JahresabschlussEntry BerechneJahr(Daten daten, int jahr)
        {
            // Konten von Verträgen mit Abrechnungsverzicht für dieses Jahr: deren offene
            // Forderungen (Miete/NK/BK-Abrechnung) zählen dann nicht als offener Punkt —
            // auf die Abrechnung wird ja bewusst verzichtet.
            var verzichteteVertragIds = daten.Verzichte
                .Where(v => v.Jahr == jahr)
                .Select(v => v.Vertrag.VertragId)
                .ToHashSet();
            var verzichteteKontoIds = daten.Vertraege
                .Where(v => verzichteteVertragIds.Contains(v.VertragId))
                .SelectMany(v => new[]
                {
                    v.MietBuchungskonto.BuchungskontoId,
                    v.NkBuchungskonto.BuchungskontoId,
                    v.BkAbrechnungsKonto.BuchungskontoId,
                    v.ZahlungsKonto.BuchungskontoId,
                    v.MietminderungsKonto.BuchungskontoId
                })
                .ToHashSet();

            var konten = daten.Konten
                .Where(k => k.Buchungszeilen.Any(z => z.Buchungssatz.Buchungsjahr <= jahr))
                .Select(k => BerechneKonto(daten, k, jahr, verzichteteKontoIds))
                .OrderBy(k => k.Ausgleichbar ? 0 : 1)
                .ThenBy(k => k.Funktion)
                .ThenBy(k => k.Kontonummer)
                .ToList();

            var abrechnungen = BerechneAbrechnungen(daten, jahr);

            var kontenOffen = konten.Count(k => !k.Ausgeglichen);
            // Erledigt = abgesendet ODER bewusster Verzicht (dokumentiert, ohne Buchung).
            var abrechnungenFertig = abrechnungen.Count(a => a.Abgesendet || a.Verzichtet);
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

        private static KontoJahresEntry BerechneKonto(
            Daten daten, Buchungskonto konto, int jahr, HashSet<int> verzichteteKontoIds)
        {
            var verknuepfung = daten.Verknuepfungen
                .FirstOrDefault(v => v.KontoId == konto.BuchungskontoId);
            var ausgleichbar = verknuepfung?.Ausgleichbar ?? false;
            var verzichtet = verzichteteKontoIds.Contains(konto.BuchungskontoId);

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

            // Ausgleichsbasierte Sicht: eine Forderung gilt als erledigt,
            // sobald sie per OPOS gedeckt ist.
            decimal OffenerRest(Buchungszeile z, Dictionary<Guid, List<OffenerPostenAusgleich>> ausgleiche,
                Func<OffenerPostenAusgleich, decimal> gegenBetrag)
            {
                var gedeckt = ausgleiche.TryGetValue(z.BuchungszeileId, out var liste)
                    ? liste.Sum(gegenBetrag)
                    : 0m;
                return z.Betrag - gedeckt;
            }

            bool IstStorniertOderStorno(Buchungszeile z) =>
                z.Buchungssatz.StornoVon != null || z.Buchungssatz.StornoNach != null;

            var offeneSoll = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungssatz.Buchungsjahr == jahr && !IstStorniertOderStorno(z))
                .Select(z => OffenerRest(z, daten.AusgleicheBySollZeile, a => a.HabenZeile.Betrag))
                .Where(offen => offen > Epsilon)
                .ToList();
            var offeneHaben = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungssatz.Buchungsjahr == jahr && !IstStorniertOderStorno(z))
                .Select(z => OffenerRest(z, daten.AusgleicheByHabenZeile, a => a.SollZeile.Betrag))
                .Where(offen => offen > Epsilon)
                .ToList();
            var offenePosten = offeneSoll.Concat(offeneHaben).ToList();

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
                Verzichtet = verzichtet,
                // Ausgeglichen, wenn keine ungedeckte Forderung (Soll oder Haben) mehr
                // offen ist. Verzichtete Konten zählen — wie Summenkonten — nicht als
                // offener Punkt.
                Ausgeglichen = verzichtet || !ausgleichbar || offenePosten.Count == 0
            };
        }

        /// <summary>
        /// True wenn der Abrechnungs-Saldo per OPOS gedeckt ist (bzw. 0 war). Der
        /// Saldo steht auf dem BkAbrechnungsKonto: Soll = Nachzahlungs-Forderung
        /// (gedeckt durch Zahlungs-Haben-Zeilen), Haben = Guthaben-Verbindlichkeit
        /// (gedeckt durch Auszahlungs-Soll-Zeilen).
        /// </summary>
        private static bool SaldoAusgeglichen(Abrechnungsresultat resultat, Vertrag vertrag)
        {
            var bkAbrKontoId = vertrag.BkAbrechnungsKonto.BuchungskontoId;
            var saldoZeilen = resultat.Buchungssatz.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == bkAbrKontoId)
                .ToList();

            var nachzahlungsZeile = saldoZeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Soll);
            if (nachzahlungsZeile != null)
            {
                var gedeckt = nachzahlungsZeile.AlsSollZeile.Sum(a => a.HabenZeile.Betrag);
                return nachzahlungsZeile.Betrag - gedeckt <= 0.005m;
            }

            var guthabenZeile = saldoZeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Haben);
            if (guthabenZeile != null)
            {
                var gedeckt = guthabenZeile.AlsHabenZeile.Sum(a => a.SollZeile.Betrag);
                return guthabenZeile.Betrag - gedeckt <= 0.005m;
            }

            return true; // kein Saldo-Leg → Saldo 0
        }

        /// <summary>Vertrag ist in <paramref name="jahr"/> aktiv (Version begonnen, nicht vor Jahresbeginn beendet).</summary>
        private static bool IstAktivImJahr(Vertrag vertrag, int jahr)
        {
            var startOfYear = new DateOnly(jahr, 1, 1);
            var endOfYear = new DateOnly(jahr, 12, 31);
            return vertrag.Versionen.Any(ver => ver.Beginn <= endOfYear)
                && (vertrag.Ende == null || vertrag.Ende >= startOfYear);
        }

        private static List<AbrechnungsstatusEntry> BerechneAbrechnungen(Daten daten, int jahr)
        {
            return daten.Vertraege
                .Where(v => IstAktivImJahr(v, jahr))
                .Select(v =>
                {
                    // Stornierte Abrechnungssätze zählen nicht als vorhanden.
                    var resultat = daten.Resultate
                        .Where(r => r.Vertrag.VertragId == v.VertragId
                                 && r.Buchungssatz.Buchungsjahr == jahr
                                 && r.Buchungssatz.StornoNach == null)
                        .MaxBy(r => r.LastModified);
                    var verzicht = daten.Verzichte
                        .FirstOrDefault(vz => vz.Vertrag.VertragId == v.VertragId && vz.Jahr == jahr);
                    return new AbrechnungsstatusEntry
                    {
                        VertragId = v.VertragId,
                        Bezeichnung = KontoVerknuepfungService.GetVertragName(v),
                        ResultatVorhanden = resultat != null,
                        Abgesendet = resultat?.Abgesendet ?? false,
                        Ausgeglichen = resultat != null && SaldoAusgeglichen(resultat, v),
                        BuchungssatzId = resultat?.Buchungssatz.BuchungssatzId,
                        Verzichtet = verzicht != null,
                        VerzichtGrund = verzicht?.Grund,
                        VerzichtId = verzicht?.AbrechnungsverzichtId
                    };
                })
                .OrderBy(a => a.Bezeichnung)
                .ToList();
        }
    }
}
