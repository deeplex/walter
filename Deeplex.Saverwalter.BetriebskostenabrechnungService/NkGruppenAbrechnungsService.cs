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

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    /// <summary>
    /// Gruppenbasierte NK-Berechnung ohne Abhängigkeit auf die
    /// Legacy-<c>Betriebskostenabrechnung</c>-Klasse.
    ///
    /// Kernidee:
    ///   Umlagen werden nach Wohnungskombination (= Einheit) gruppiert.
    ///   Pro Einheit wird für jede (Wohnung, Vertrag?)-Kombination eine <see cref="NkPartei"/>
    ///   erstellt.  Ist <see cref="NkPartei.Vertrag"/> null, handelt es sich um einen
    ///   Eigenanteil / Leerstand – dieser wird symmetrisch behandelt wie ein Mieter-Anteil.
    ///
    ///   <see cref="NkPartei.GetAnteil"/> liefert für Eigenanteil-Parteien:
    ///     – WF/NF/NE/MEA: den proportionalen Zeitanteil des Leerstands
    ///     – Personenzahl:  0 (keine Personen im Leerstand)
    ///     – Verbrauch:     abhängig von Zählerständen im Leerstand-Zeitraum
    ///
    ///   Alle Parteien (Mieter + Eigenanteil) werden in <see cref="BuildRechnungsplaene"/>
    ///   einheitlich behandelt – keine separate Lücken-Arithmetik.
    ///
    /// Unterstützte Schlüssel: WF, NF, NE, MEA, Personenzahl, Verbrauch.
    /// Heizkosten-Abzug (HKVO/Strompauschale) wird nicht angewendet –
    /// HKVO-Umlagen (§7/§8/§9) werden korrekt aufgeteilt.
    ///
    /// Alle Methoden sind rein funktional (kein DB-Zugriff).
    /// Benötigte Includes auf Umlage (caller-seitig):
    ///   Wohnungen → Vertraege → Versionen
    ///   Wohnungen → Vertraege → Mieter
    ///   Wohnungen → Vertraege → NkBuchungskonto
    ///   Wohnungen → AufwandsKonto
    ///   Zaehler   → Staende
    ///   Zaehler   → Wohnung
    ///   HKVO      (für warme Betriebskosten §7/§8/§9)
    ///   Betriebskostenrechnungen → Buchungssatz → Buchungszeilen → Buchungskonto
    /// </summary>
    public static class NkGruppenAbrechnungsService
    {
        // ── Öffentliche Datenmodelle ──────────────────────────────────────────

        /// <summary>
        /// Eine Einheit: alle Wohnungen, die exakt dieselben Umlagen teilen.
        /// <see cref="Parteien"/> enthält alle Mieter- und Eigenanteil-Parteien;
        /// die beteiligten Wohnungen sind über <c>Parteien.Select(p => p.Wohnung).DistinctBy(...)</c>
        /// ableitbar.
        /// </summary>
        public sealed class NkEinheit
        {
            public string Bezeichnung { get; init; } = "";
            public IReadOnlyList<Umlage> Umlagen { get; init; } = [];
            public IReadOnlyList<NkPartei> Parteien { get; init; } = [];
            public IReadOnlyList<NkRechnungsplan> Rechnungsplaene { get; init; } = [];
        }

        /// <summary>
        /// Finanzielle Kennzahlen eines Vertrags im Abrechnungsjahr.
        /// Wird auf <see cref="NkPartei.VertragInfo"/> gesetzt; für Eigenanteil-Parteien null.
        /// </summary>
        public sealed class NkVertragInfo
        {
            /// <summary>Im Jahr geleistete NK-Vorauszahlungen (Haben auf NkBuchungskonto).</summary>
            public decimal Vorauszahlung { get; init; }

            /// <summary>Mietsoll minus Miethaben im Jahr.</summary>
            public decimal MietSaldo { get; init; }

            /// <summary>
            /// Bereits gebuchtes Abrechnungsresultat, oder null wenn noch nicht gebucht.
            /// </summary>
            public Abrechnungsresultat? ExistingResultat { get; init; }

            /// <summary>
            /// Bereits gebuchter Rechnungsbetrag (Haben auf BkAbrechnungsKonto), oder null wenn noch nicht gebucht.
            /// Ermöglicht Vergleich mit dem aktuell berechneten Rechnungsbetrag.
            /// </summary>
            public decimal? GebuchtesAbrechnungsResultat { get; init; }
        }

        /// <summary>
        /// Eine (Wohnung, Vertrag?)-Partei mit vorberechneten Anteil-Faktoren.
        /// <see cref="Vertrag"/> == null → Eigenanteil / Leerstand.
        ///
        /// Eigenanteil-Parteien haben:
        ///   WF/NF/NE/MEA-Zeitanteile proportional zum Leerstand-Zeitanteil,
        ///   PersonenZeitanteile = [] → GetAnteil für Personenzahl = 0,
        ///   VerbrauchAnteilByUmlageId gefüllt aus Zählerständen im Leerstand-Zeitraum.
        /// </summary>
        public sealed class NkPartei
        {
            public Wohnung Wohnung { get; init; } = null!;

            /// <summary>null für Eigenanteil / Leerstand.</summary>
            public Vertrag? Vertrag { get; init; }

            /// <summary>Erster Nutzungstag im Abrechnungsjahr.</summary>
            public DateOnly Nutzungsbeginn { get; init; }
            /// <summary>Letzter Nutzungstag im Abrechnungsjahr.</summary>
            public DateOnly Nutzungsende { get; init; }

            /// <summary>Anteil der Nutzungstage an den Jahrestagen (0–1).</summary>
            public decimal Zeitanteil { get; init; }

            // Statische Anteile (Zeitanteil bereits eingerechnet)
            public decimal WFZeitanteil { get; init; }
            public decimal NFZeitanteil { get; init; }
            public decimal NEZeitanteil { get; init; }
            public decimal MEAZeitanteil { get; init; }

            /// <summary>Leer für Eigenanteil-Parteien → GetAnteil für Personenzahl = 0.</summary>
            public IReadOnlyList<PersonenZeitanteil> PersonenZeitanteile { get; init; } = [];

            /// <summary>
            /// Verbrauchsanteile je Umlage-Id (Skalar, für Berechnung / GetAnteil).
            /// Für Eigenanteil-Parteien inline befüllt; für Mieter-Parteien aus VerbrauchAnteileDetail abgeleitet.
            /// </summary>
            public IReadOnlyDictionary<int, decimal> VerbrauchAnteilByUmlageId { get; init; }
                = new Dictionary<int, decimal>();

            /// <summary>
            /// Vollständige VerbrauchAnteil-Objekte je Umlage-Id (DieseZaehler/AlleZaehler/Anteil).
            /// Nur für Mieter-Parteien befüllt; leer für Eigenanteil/Leerstand.
            /// Wird für die Detaildarstellung im Druckdokument benötigt.
            /// </summary>
            public IReadOnlyDictionary<int, VerbrauchAnteil> VerbrauchAnteileDetail { get; init; }
                = new Dictionary<int, VerbrauchAnteil>();

            /// <summary>
            /// Vorauszahlung, MietSaldo und Buchungsstatus des Vertrags im Abrechnungsjahr.
            /// null für Eigenanteil-Parteien.
            /// </summary>
            public NkVertragInfo? VertragInfo { get; init; }

            /// <summary>
            /// Das Buchungskonto für NK-Anteil-Buchungen:
            /// Mieter-Partei → Vertrag.NkBuchungskonto,
            /// Eigenanteil   → Wohnung.AufwandsKonto.
            /// </summary>
            public Buchungskonto Buchungskonto => Vertrag?.NkBuchungskonto ?? Wohnung.AufwandsKonto;

            /// <summary>Anteil dieser Partei an einer Umlage (0–1).</summary>
            public decimal GetAnteil(Umlage umlage) => umlage.Schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => WFZeitanteil,
                Umlageschluessel.NachNutzflaeche => NFZeitanteil,
                Umlageschluessel.NachNutzeinheit => NEZeitanteil,
                Umlageschluessel.NachMiteigentumsanteil => MEAZeitanteil,
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteile.Sum(p => p.Anteil),
                Umlageschluessel.NachVerbrauch =>
                    VerbrauchAnteilByUmlageId.GetValueOrDefault(umlage.UmlageId),
                _ => 0
            };
        }

        /// <summary>Berechnete Anteilverteilung für eine Betriebskostenrechnung.</summary>
        public sealed class NkRechnungsplan
        {
            public Betriebskostenrechnung Rechnung { get; init; } = null!;
            public Umlage Umlage { get; init; } = null!;
            public IReadOnlyList<NkRechnungsAnteil> Anteile { get; init; } = [];
            public IReadOnlyList<string> Warnungen { get; init; } = [];

            /// <summary>§9(2)-Warmwasseranteil. Nur für HKVO-Umlagen gesetzt.</summary>
            public decimal? Para9_2 { get; init; }
            /// <summary>Gesamtverbrauch Wärme aller Parteien (kWh). Nur für HKVO-Umlagen.</summary>
            public decimal? GesamtWaerme { get; init; }
            /// <summary>Gesamtverbrauch Warmwasser aller Parteien (m³). Nur für HKVO-Umlagen.</summary>
            public decimal? GesamtWW { get; init; }
            /// <summary>
            /// Individuelle Heiz- und WW-Verbrauchsanteile und Zähler-Einzelwerte je Partei.
            /// Key = NkPartei-Instanz; nur für HKVO-Umlagen mit Wohnungszählern befüllt.
            /// </summary>
            public IReadOnlyDictionary<NkPartei, NkHkvoParteiVerbrauch>? HkvoVerbrauchAnteile { get; init; }
        }

        /// <summary>
        /// Verbrauchsanteile und Zähler-Einzelwerte einer Partei für eine HKVO-Umlage.
        /// </summary>
        public sealed record NkHkvoParteiVerbrauch(
            decimal HeizAnteil,
            decimal WWAnteil,
            IReadOnlyList<(Zaehler Zaehler, decimal Verbrauch)> HeizZaehler,
            IReadOnlyList<(Zaehler Zaehler, decimal Verbrauch)> WwZaehler)
        {
            public decimal HeizVerbrauchGesamt => HeizZaehler.Sum(x => x.Verbrauch);
            public decimal WwVerbrauchGesamt   => WwZaehler.Sum(x => x.Verbrauch);
        }

        /// <summary>Anteil einer Partei an einer Rechnung.</summary>
        public sealed record NkRechnungsAnteil
        {
            public NkPartei Partei { get; init; } = null!;
            public decimal Betrag { get; init; }
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>
        /// Gruppiert Umlagen nach Wohnungskombination und berechnet pro Einheit
        /// alle NK-Anteile inkl. Eigenanteile. Keine DB-Zugriffe.
        /// </summary>
        public static List<NkEinheit> ComputeEinheiten(IEnumerable<Umlage> umlagen, int jahr)
        {
            var abrechnungsbeginn = new DateOnly(jahr, 1, 1);
            var abrechnungsende = new DateOnly(jahr, 12, 31);
            var abrechnungstage = abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1;

            return [.. umlagen
                .GroupBy(u => string.Join(",", u.Wohnungen.Select(w => w.WohnungId).OrderBy(id => id)))
                .Select(group =>
                    BuildEinheit(group.ToList(), jahr, abrechnungsbeginn, abrechnungsende, abrechnungstage))];
        }

        // ── Interne Berechnung ────────────────────────────────────────────────

        private static NkEinheit BuildEinheit(
            List<Umlage> umlagenInGruppe,
            int jahr,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende,
            int abrechnungstage)
        {
            var wohnungen = umlagenInGruppe.First().Wohnungen
                .OrderBy(w => w.WohnungId)
                .ToList();

            var gesamtWF = wohnungen.Sum(w => w.Wohnflaeche);
            var gesamtNF = wohnungen.Sum(w => w.Nutzflaeche);
            var gesamtNE = wohnungen.Sum(w => (decimal)w.Nutzeinheit);
            var gesamtMEA = wohnungen.Sum(w => w.Miteigentumsanteile);

            var parteien = BuildParteien(
                wohnungen, umlagenInGruppe,
                gesamtWF, gesamtNF, gesamtNE, gesamtMEA,
                abrechnungsbeginn, abrechnungsende, abrechnungstage, jahr);

            var rechnungsplaene = BuildRechnungsplaene(umlagenInGruppe, parteien, abrechnungsbeginn, abrechnungsende);

            return new NkEinheit
            {
                Bezeichnung = wohnungen.GetWohnungenBezeichnung(),
                Umlagen = umlagenInGruppe,
                Parteien = parteien,
                Rechnungsplaene = rechnungsplaene
            };
        }

        /// <summary>
        /// Erstellt alle Parteien einer Einheit: Vertrags-Parteien + Eigenanteil-Parteien.
        ///
        /// Eigenanteile werden über das Intervall-Komplement berechnet:
        /// Die durch Verträge belegten Zeiträume werden von [Jan 1, Dez 31] subtrahiert;
        /// jeder verbleibende Leerstand-Abschnitt wird als eigene NkPartei modelliert.
        /// Das ergibt korrekte <c>Nutzungsbeginn/Nutzungsende</c> pro Leerstand-Periode
        /// und ermöglicht eine Verbrauchsberechnung über denselben Zeitraum.
        /// </summary>
        private static List<NkPartei> BuildParteien(
            List<Wohnung> wohnungen,
            List<Umlage> umlagenInGruppe,
            decimal gesamtWF,
            decimal gesamtNF,
            decimal gesamtNE,
            decimal gesamtMEA,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende,
            int abrechnungstage,
            int jahr)
        {
            var parteien = new List<NkPartei>();

            foreach (var wohnung in wohnungen)
            {
                // Vertrags-Parteien für diese Wohnung
                foreach (var vertrag in wohnung.Vertraege.Where(v => VertragAktivInJahr(v, jahr)))
                {
                    var partei = BuildVertragsPartei(
                        vertrag, wohnung, wohnungen, umlagenInGruppe,
                        gesamtWF, gesamtNF, gesamtNE, gesamtMEA,
                        abrechnungsbeginn, abrechnungsende, abrechnungstage, jahr);
                    if (partei is not null)
                        parteien.Add(partei);
                }

                // Eigenanteil-Parteien: eine NkPartei je Leerstand-Intervall.
                // WF/NF/NE/MEA ergeben sich direkt aus dem Zeitanteil des Intervalls.
                // Personenzahl = 0 (leer → keine Personen).
                // Verbrauch: wird über denselben Zeitraum aus den Zählern berechnet,
                //   da z.B. Grundverbrauch oder Allgemeinstrom auch im Leerstand anfällt.
                var belegteIntervalle = parteien
                    .Where(p => p.Wohnung.WohnungId == wohnung.WohnungId)
                    .Select(p => (p.Nutzungsbeginn, p.Nutzungsende))
                    .ToList();

                foreach (var (lsBeginn, lsEnde) in FindLeerstandIntervalle(
                    belegteIntervalle, abrechnungsbeginn, abrechnungsende))
                {
                    var lsTage = lsEnde.DayNumber - lsBeginn.DayNumber + 1;
                    var lsZeitanteil = (decimal)lsTage / abrechnungstage;

                    var notes = new List<Note>();
                    var verbrauchAnteile = umlagenInGruppe
                        .Where(u => u.Schluessel == Umlageschluessel.NachVerbrauch && u.Zaehler.Count > 0)
                        .Select(u =>
                        {
                            // VerbrauchAnteil requires a Vertrag-based Zeitraum, which doesn't exist for Leerstand.
                            // We compute the anteil inline: DieseVerbrauch / AlleVerbrauch per Einheit.
                            var zaehlerMitWohnung = u.Zaehler.Where(z => z.Wohnung != null).ToList();
                            var alleVerbrauch = zaehlerMitWohnung
                                .GroupBy(z => z.Typ.ToUnit())
                                .ToDictionary(
                                    g => g.Key,
                                    g => g.Sum(z => new Verbrauch(z, abrechnungsbeginn, abrechnungsende, notes).Delta));
                            var dieseVerbrauch = zaehlerMitWohnung
                                .Where(z => z.Wohnung == wohnung)
                                .GroupBy(z => z.Typ.ToUnit())
                                .ToDictionary(
                                    g => g.Key,
                                    g => g.Sum(z => new Verbrauch(z, lsBeginn, lsEnde, notes).Delta));
                            var anteil = dieseVerbrauch
                                .Where(kv => alleVerbrauch.TryGetValue(kv.Key, out var alle) && alle > 0)
                                .Sum(kv => kv.Value / alleVerbrauch[kv.Key]);
                            return (UmlageId: u.UmlageId, Anteil: anteil);
                        })
                        .Where(x => x.Anteil > 0)
                        .ToDictionary(x => x.UmlageId, x => x.Anteil);

                    parteien.Add(new NkPartei
                    {
                        Wohnung = wohnung,
                        Vertrag = null,
                        Nutzungsbeginn = lsBeginn,
                        Nutzungsende = lsEnde,
                        Zeitanteil = lsZeitanteil,
                        WFZeitanteil = gesamtWF > 0 ? lsZeitanteil * wohnung.Wohnflaeche / gesamtWF : 0,
                        NFZeitanteil = gesamtNF > 0 ? lsZeitanteil * wohnung.Nutzflaeche / gesamtNF : 0,
                        NEZeitanteil = gesamtNE > 0 ? lsZeitanteil * wohnung.Nutzeinheit / gesamtNE : 0,
                        MEAZeitanteil = gesamtMEA > 0
                            ? lsZeitanteil * wohnung.Miteigentumsanteile / gesamtMEA : 0,
                        VerbrauchAnteilByUmlageId = verbrauchAnteile,
                        // PersonenZeitanteile bleibt leer → GetAnteil(Personenzahl) = 0
                    });
                }
            }

            return parteien;
        }

        /// <summary>
        /// Gibt die durch Verträge nicht belegten Intervalle innerhalb des Abrechnungsjahres zurück.
        /// Überlappende Belegungs-Intervalle werden vor der Komplement-Bildung zusammengeführt.
        /// </summary>
        private static IEnumerable<(DateOnly Beginn, DateOnly Ende)> FindLeerstandIntervalle(
            List<(DateOnly Beginn, DateOnly Ende)> belegteIntervalle,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende)
        {
            // Sortieren + überlappende Intervalle zusammenführen
            var merged = belegteIntervalle
                .OrderBy(i => i.Beginn)
                .Aggregate(new List<(DateOnly Beginn, DateOnly Ende)>(), (acc, cur) =>
                {
                    if (acc.Count > 0 && cur.Beginn <= acc[^1].Ende.AddDays(1))
                    {
                        acc[^1] = (acc[^1].Beginn, DateOnly.FromDayNumber(
                            Math.Max(acc[^1].Ende.DayNumber, cur.Ende.DayNumber)));
                    }
                    else
                    {
                        acc.Add(cur);
                    }
                    return acc;
                });

            // Komplement: Lücken zwischen und um die belegten Intervalle
            var cursor = abrechnungsbeginn;
            foreach (var (belegBeginn, belegEnde) in merged)
            {
                if (cursor < belegBeginn)
                    yield return (cursor, belegBeginn.AddDays(-1));
                cursor = belegEnde.AddDays(1);
            }
            if (cursor <= abrechnungsende)
                yield return (cursor, abrechnungsende);
        }

        private static NkPartei? BuildVertragsPartei(
            Vertrag vertrag,
            Wohnung wohnung,
            List<Wohnung> alleWohnungenDerEinheit,
            List<Umlage> umlagen,
            decimal gesamtWF,
            decimal gesamtNF,
            decimal gesamtNE,
            decimal gesamtMEA,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende,
            int abrechnungstage,
            int jahr)
        {
            var nutzungsbeginn = DateOnly.FromDayNumber(
                Math.Max(vertrag.Beginn().DayNumber, abrechnungsbeginn.DayNumber));
            var nutzungsende = DateOnly.FromDayNumber(
                Math.Min((vertrag.Ende ?? abrechnungsende).DayNumber, abrechnungsende.DayNumber));

            if (nutzungsende < nutzungsbeginn)
                return null;

            int nutzungstage = nutzungsende.DayNumber - nutzungsbeginn.DayNumber + 1;
            decimal zeitanteil = (decimal)nutzungstage / abrechnungstage;

            var zeitraum = new Zeitraum(jahr, vertrag);
            var notes = new List<Note>();

            var personenZeitanteile = PersonenZeitanteil.GetPersonenZeitanteile(
                vertrag, alleWohnungenDerEinheit, zeitraum);

            var verbrauchAnteileDetail = umlagen
                .Where(u => u.Schluessel == Umlageschluessel.NachVerbrauch && u.Zaehler.Count > 0)
                .Select(u => new VerbrauchAnteil(u, wohnung, zeitraum, notes))
                .Where(va => va.Anteil.Values.Sum() > 0)
                .ToDictionary(va => va.Umlage.UmlageId, va => va);

            var verbrauchAnteile = verbrauchAnteileDetail
                .ToDictionary(kv => kv.Key, kv => kv.Value.Anteil.Values.Sum());

            // Buchungszeilen sind durch den Include bereits auf das Abrechnungsjahr gefiltert.
            var existing = vertrag.Abrechnungsresultate
                .FirstOrDefault(r => r.Buchungssatz.Buchungsdatum.Year == jahr);
            var gebuchtesResultat = existing?.Buchungssatz.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                            && z.Buchungskonto.BuchungskontoId == vertrag.BkAbrechnungsKonto.BuchungskontoId)
                .Sum(z => z.Betrag);
            var vertragInfo = new NkVertragInfo
            {
                Vorauszahlung = vertrag.NkBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag),
                MietSaldo = vertrag.MietBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag)
                    - vertrag.MietBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag),
                ExistingResultat = existing,
                GebuchtesAbrechnungsResultat = gebuchtesResultat,
            };

            return new NkPartei
            {
                Wohnung = wohnung,
                Vertrag = vertrag,
                Nutzungsbeginn = nutzungsbeginn,
                Nutzungsende = nutzungsende,
                Zeitanteil = zeitanteil,
                WFZeitanteil = gesamtWF > 0 ? zeitanteil * wohnung.Wohnflaeche / gesamtWF : 0,
                NFZeitanteil = gesamtNF > 0 ? zeitanteil * wohnung.Nutzflaeche / gesamtNF : 0,
                NEZeitanteil = gesamtNE > 0 ? zeitanteil * wohnung.Nutzeinheit / gesamtNE : 0,
                MEAZeitanteil = gesamtMEA > 0 ? zeitanteil * wohnung.Miteigentumsanteile / gesamtMEA : 0,
                PersonenZeitanteile = personenZeitanteile,
                VerbrauchAnteilByUmlageId = verbrauchAnteile,
                VerbrauchAnteileDetail = verbrauchAnteileDetail,
                VertragInfo = vertragInfo,
            };
        }

        /// <summary>
        /// Alle Parteien (Mieter + Eigenanteil) werden einheitlich behandelt.
        /// HKVO-Umlagen (§7/§8/§9) erhalten eine spezielle Verteilung.
        /// </summary>
        private static List<NkRechnungsplan> BuildRechnungsplaene(
            List<Umlage> umlagen,
            List<NkPartei> parteien,
            DateOnly beginn,
            DateOnly ende)
        {
            var result = new List<NkRechnungsplan>();

            foreach (var umlage in umlagen)
            {
                if (umlage.HKVO != null)
                {
                    ProcessHkvoUmlage(umlage, parteien, result, beginn, ende);
                    continue;
                }

                foreach (var rechnung in umlage.Betriebskostenrechnungen)
                {
                    var warnungen = new List<string>();
                    var anteile = new List<NkRechnungsAnteil>();

                    foreach (var partei in parteien)
                    {
                        decimal anteilFaktor = partei.GetAnteil(umlage);
                        if (anteilFaktor <= 0) continue;

                        decimal betrag = Math.Round(rechnung.Betrag * anteilFaktor, 2);
                        if (betrag <= 0) continue;

                        anteile.Add(new NkRechnungsAnteil { Partei = partei, Betrag = betrag });
                    }

                    ApplyRundungskorrektur(rechnung.Betrag, anteile);

                    result.Add(new NkRechnungsplan
                    {
                        Rechnung = rechnung,
                        Umlage = umlage,
                        Anteile = anteile,
                        Warnungen = warnungen
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// HKVO §7/§8/§9: Aufteilung der Heizkosten nach warmer und kalter Fraktion.
        ///
        /// §9(2): Warmwasseranteil P = 2,5 × (V/Q) × (tw − 10°C)
        ///   V = Allgemein-Warmwasser (m³), Q = Allgemein-Wärmequellen (kWh)
        ///
        /// §7: Heizungsanteil (1−P) × Gesamtbetrag
        ///   P7 % nach individuellem Wärmeverbrauch, (1−P7) % nach WF×Zeitanteil
        ///
        /// §8: Warmwasseranteil P × Gesamtbetrag
        ///   P8 % nach individuellem WW-Verbrauch, (1−P8) % nach WF×Zeitanteil
        ///
        /// Sind keine individuellen Zähler vorhanden, fällt die verbrauchsabhängige
        /// Quote vollständig auf WF×Zeitanteil zurück.
        /// </summary>
        private static void ProcessHkvoUmlage(
            Umlage umlage,
            List<NkPartei> parteien,
            List<NkRechnungsplan> result,
            DateOnly beginn,
            DateOnly ende)
        {
            var hkvo = umlage.HKVO!;
            var p7 = hkvo.HKVO_P7;
            var p8 = hkvo.HKVO_P8;
            var notes = new List<Note>();

            // Individuelle Wohnungszähler für §7 (Wärme) und §8 (WW)
            var wohnungWaerme = umlage.Zaehler
                .Where(z => z.Wohnung != null && IsWärmequelle(z.Typ))
                .ToList();
            var wohnungWW = umlage.Zaehler
                .Where(z => z.Wohnung != null && z.Typ == Zaehlertyp.Warmwasser)
                .ToList();

            // §9(2): Q vom HKVO-Allgemeinzähler, V = Summe aller Wohnungs-WW-Zähler
            decimal para9_2 = 0;
            if (hkvo.AllgemeinWaerme != null)
            {
                var Q = new Verbrauch(hkvo.AllgemeinWaerme, beginn, ende, notes).Delta;
                var V = wohnungWW.Sum(z => new Verbrauch(z, beginn, ende, notes).Delta);
                if (Q > 0 && V > 0)
                {
                    const decimal tw = 60m;
                    para9_2 = 2.5m * (V / Q) * (tw - 10m);
                }
            }

            // Verbrauch pro Partei – je Zähler einzeln (Nutzungszeitraum der Partei als Messfenster)
            var waermeZaehlerByPartei = parteien.ToDictionary(
                p => p,
                p => (IReadOnlyList<(Zaehler, decimal)>)wohnungWaerme
                    .Where(z => z.Wohnung!.WohnungId == p.Wohnung.WohnungId)
                    .Select(z => (z, new Verbrauch(z, p.Nutzungsbeginn, p.Nutzungsende, notes).Delta))
                    .ToList());
            var wwZaehlerByPartei = parteien.ToDictionary(
                p => p,
                p => (IReadOnlyList<(Zaehler, decimal)>)wohnungWW
                    .Where(z => z.Wohnung!.WohnungId == p.Wohnung.WohnungId)
                    .Select(z => (z, new Verbrauch(z, p.Nutzungsbeginn, p.Nutzungsende, notes).Delta))
                    .ToList());

            var totalWaerme = waermeZaehlerByPartei.Values.SelectMany(x => x).Sum(x => x.Item2);
            var totalWW     = wwZaehlerByPartei.Values.SelectMany(x => x).Sum(x => x.Item2);

            // Individuelle Verbrauchsanteile und Zähler-Einzelwerte je Partei
            var hkvoVerbrauchAnteile = parteien.ToDictionary(
                p => p,
                p =>
                {
                    var heizGesamt = waermeZaehlerByPartei[p].Sum(x => x.Item2);
                    var wwGesamt   = wwZaehlerByPartei[p].Sum(x => x.Item2);
                    return new NkHkvoParteiVerbrauch(
                        HeizAnteil:  totalWaerme > 0 ? heizGesamt / totalWaerme : 0m,
                        WWAnteil:    totalWW    > 0 ? wwGesamt   / totalWW    : 0m,
                        HeizZaehler: waermeZaehlerByPartei[p],
                        WwZaehler:   wwZaehlerByPartei[p]);
                });

            foreach (var rechnung in umlage.Betriebskostenrechnungen)
            {
                var betragHZ = rechnung.Betrag * (1 - para9_2);
                var betragWW = rechnung.Betrag * para9_2;
                var anteile = new List<NkRechnungsAnteil>();

                foreach (var partei in parteien)
                {
                    var hkvoPartei = hkvoVerbrauchAnteile[partei];

                    decimal heizKostenAnteil = totalWaerme > 0
                        ? p7 * hkvoPartei.HeizAnteil + (1 - p7) * partei.WFZeitanteil
                        : partei.WFZeitanteil;

                    decimal wwKostenAnteil = totalWW > 0
                        ? p8 * hkvoPartei.WWAnteil + (1 - p8) * partei.WFZeitanteil
                        : partei.WFZeitanteil;

                    decimal meinBetrag = Math.Round(betragHZ * heizKostenAnteil + betragWW * wwKostenAnteil, 2);
                    if (meinBetrag <= 0) continue;

                    anteile.Add(new NkRechnungsAnteil { Partei = partei, Betrag = meinBetrag });
                }

                ApplyRundungskorrektur(rechnung.Betrag, anteile);

                result.Add(new NkRechnungsplan
                {
                    Rechnung = rechnung,
                    Umlage = umlage,
                    Anteile = anteile,
                    Warnungen = [],
                    Para9_2 = para9_2 > 0 ? para9_2 : null,
                    GesamtWaerme = totalWaerme > 0 ? totalWaerme : null,
                    GesamtWW = totalWW > 0 ? totalWW : null,
                    HkvoVerbrauchAnteile = hkvoVerbrauchAnteile,
                });
            }
        }

        // Bevorzugt letzten Eigenanteil als Träger der Rundungsdifferenz.
        private static void ApplyRundungskorrektur(decimal sollBetrag, List<NkRechnungsAnteil> anteile)
        {
            decimal diff = sollBetrag - anteile.Sum(a => a.Betrag);
            if (Math.Abs(diff) <= 0.005m || anteile.Count == 0) return;

            int targetIdx = anteile.FindLastIndex(a => a.Partei.Vertrag is null);
            if (targetIdx < 0) targetIdx = anteile.Count - 1;
            var target = anteile[targetIdx];
            anteile[targetIdx] = target with { Betrag = target.Betrag + diff };
        }

        /// <summary>Gibt true für alle Energieträger-Zählertypen zurück (trägerunabhängig).</summary>
        internal static bool IsWärmequelle(Zaehlertyp typ) =>
            typ is Zaehlertyp.Gas or Zaehlertyp.Wärme;

        private static bool VertragAktivInJahr(Vertrag vertrag, int jahr)
        {
            if (vertrag.Versionen.Count == 0) return false;
            var beginn = vertrag.Versionen.Min(v => v.Beginn);
            if (beginn.Year > jahr) return false;
            if (vertrag.Ende.HasValue && vertrag.Ende.Value.Year < jahr) return false;
            return true;
        }
    }
}
