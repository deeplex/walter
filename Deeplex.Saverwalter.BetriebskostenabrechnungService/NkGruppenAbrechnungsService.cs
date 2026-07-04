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
    ///   NkVerrechnungsKonto → Buchungszeilen (Haben) → Buchungssatz → Buchungszeilen → Buchungskonto
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
            /// Bereits gebuchter Abrechnungs-Saldo (Glattstellung auf dem
            /// BkAbrechnungsKonto: Soll = Nachzahlung, Haben = Guthaben), oder null
            /// wenn noch nicht gebucht. Ermöglicht Vergleich mit dem aktuell
            /// berechneten Saldo (Rechnungsbetrag − Vorauszahlung).
            /// </summary>
            public decimal? GebuchterSaldo { get; init; }
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
            public decimal GetAnteil(Umlageschluessel schluessel, int umlageId) => schluessel switch
            {
                Umlageschluessel.NachWohnflaeche => WFZeitanteil,
                Umlageschluessel.NachNutzflaeche => NFZeitanteil,
                Umlageschluessel.NachNutzeinheit => NEZeitanteil,
                Umlageschluessel.NachMiteigentumsanteil => MEAZeitanteil,
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteile.Sum(p => p.Anteil),
                Umlageschluessel.NachVerbrauch =>
                    VerbrauchAnteilByUmlageId.GetValueOrDefault(umlageId),
                _ => 0
            };
        }

        /// <summary>Berechnete Anteilverteilung für einen BK-Forderungs-Buchungssatz.</summary>
        public sealed class NkRechnungsplan
        {
            public Buchungssatz Buchungssatz { get; init; } = null!;
            public decimal Betrag { get; init; }
            public Umlage Umlage { get; init; } = null!;
            public IReadOnlyList<NkRechnungsAnteil> Anteile { get; init; } = [];
            public IReadOnlyList<string> Warnungen { get; init; } = [];

            /// <summary>§9(2)-Warmwasseranteil. Nur für HKVO-Umlagen gesetzt.</summary>
            public decimal? Para9_2 { get; init; }
            /// <summary>Eingangswerte der §9(2)-Berechnung. Nur gesetzt, wenn ein Allgemeinzähler konfiguriert ist.</summary>
            public NkP9Details? P9Details { get; init; }
            /// <summary>Gesamtverbrauch Wärme aller Parteien (kWh). Nur für HKVO-Umlagen.</summary>
            public decimal? GesamtWaerme { get; init; }
            /// <summary>Gesamtverbrauch Warmwasser aller Parteien (m³). Nur für HKVO-Umlagen.</summary>
            public decimal? GesamtWW { get; init; }
            /// <summary>
            /// Individuelle Heiz- und WW-Verbrauchsanteile und Zähler-Einzelwerte je Partei.
            /// Key = NkPartei-Instanz; nur für HKVO-Umlagen mit Wohnungszählern befüllt.
            /// </summary>
            public IReadOnlyDictionary<NkPartei, NkHkvoParteiVerbrauch>? HkvoVerbrauchAnteile { get; init; }

            /// <summary>
            /// True für den Strompauschale-Umbuchungs-Plan (HeizkostenV): delta der Heizkosten
            /// wird vom Allgemeinstrom abgezogen und den Heizkosten zugeschlagen. Der
            /// <see cref="Buchungssatz"/> ist die Umbuchung (Soll Betriebsstrom-NkVK /
            /// Haben Heizkosten-NkVK); er wird beim Buchen persistiert.
            /// </summary>
            public bool IstStrompauschale { get; init; }
        }

        /// <summary>
        /// Eingangswerte der §9(2)-Berechnung: P = 2,5 × V × (Tw − 10) / Q.
        /// V = Summe der Wohnungs-Warmwasserzähler (m³) über den Abrechnungszeitraum,
        /// Q = Verbrauch des Allgemein-Wärmezählers (kWh), Messfenster in QAnfangsdatum/QEnddatum.
        /// </summary>
        public sealed record NkP9Details(
            decimal V,
            decimal Q,
            decimal Tw,
            string AllgemeinZaehlerKennnummer,
            DateOnly QAnfangsdatum,
            DateOnly QEnddatum,
            IReadOnlyList<(Zaehler Zaehler, decimal Verbrauch)> WwZaehler);

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
            public decimal WwVerbrauchGesamt => WwZaehler.Sum(x => x.Verbrauch);
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

            var gesamtWF = wohnungen.Sum(w => w.VersionAt(abrechnungsende).Wohnflaeche);
            var gesamtNF = wohnungen.Sum(w => w.VersionAt(abrechnungsende).Nutzflaeche);
            var gesamtNE = wohnungen.Sum(w => (decimal)w.VersionAt(abrechnungsende).Nutzeinheit);
            var gesamtMEA = wohnungen.Sum(w => w.VersionAt(abrechnungsende).Miteigentumsanteile);

            var parteien = BuildParteien(
                wohnungen, umlagenInGruppe,
                gesamtWF, gesamtNF, gesamtNE, gesamtMEA,
                abrechnungsbeginn, abrechnungsende, abrechnungstage, jahr);

            var rechnungsplaene = BuildRechnungsplaene(umlagenInGruppe, parteien, abrechnungsbeginn, abrechnungsende, jahr, abrechnungsende);

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
                        .Where(u => u.VersionAt(abrechnungsende).Schluessel == Umlageschluessel.NachVerbrauch && u.Zaehler.Count > 0)
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
                        WFZeitanteil = gesamtWF > 0 ? lsZeitanteil * wohnung.VersionAt(abrechnungsende).Wohnflaeche / gesamtWF : 0,
                        NFZeitanteil = gesamtNF > 0 ? lsZeitanteil * wohnung.VersionAt(abrechnungsende).Nutzflaeche / gesamtNF : 0,
                        NEZeitanteil = gesamtNE > 0 ? lsZeitanteil * wohnung.VersionAt(abrechnungsende).Nutzeinheit / gesamtNE : 0,
                        MEAZeitanteil = gesamtMEA > 0
                            ? lsZeitanteil * wohnung.VersionAt(abrechnungsende).Miteigentumsanteile / gesamtMEA : 0,
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
                .Where(u => u.VersionAt(abrechnungsende).Schluessel == Umlageschluessel.NachVerbrauch && u.Zaehler.Count > 0)
                .Select(u => new VerbrauchAnteil(u, wohnung, zeitraum, notes))
                .Where(va => va.Anteil.Values.Sum() > 0)
                .ToDictionary(va => va.Umlage.UmlageId, va => va);

            var verbrauchAnteile = verbrauchAnteileDetail
                .ToDictionary(kv => kv.Key, kv => kv.Value.Anteil.Values.Sum());

            // Buchungszeilen sind durch den Include bereits auf das Abrechnungsjahr gefiltert.
            var existing = vertrag.Abrechnungsresultate
                .FirstOrDefault(r => r.Buchungssatz.Buchungsjahr == jahr);
            // Glattstellungs-Saldo auf dem BkAbrechnungsKonto: Soll = Nachzahlung,
            // Haben = Guthaben. Leerer Satz (saldo 0) ergibt 0.
            var gebuchterSaldo = existing?.Buchungssatz.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == vertrag.BkAbrechnungsKonto.BuchungskontoId)
                .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);
            var abrechnungsSatzId = existing?.Buchungssatz.BuchungssatzId;
            var vertragInfo = new NkVertragInfo
            {
                // Geleistete NK-Vorauszahlungen (Haben auf NkBuchungskonto); die
                // eigene Glattstellungs-Haben-Zeile der Abrechnung ausklammern, sonst
                // würde der Saldo nach dem Buchen fälschlich als Konflikt erkannt.
                Vorauszahlung = vertrag.NkBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben
                             && z.Buchungssatz.BuchungssatzId != abrechnungsSatzId)
                    .Sum(z => z.Betrag),
                MietSaldo = vertrag.MietBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Soll).Sum(z => z.Betrag)
                    - vertrag.MietBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben).Sum(z => z.Betrag),
                ExistingResultat = existing,
                GebuchterSaldo = gebuchterSaldo,
            };

            return new NkPartei
            {
                Wohnung = wohnung,
                Vertrag = vertrag,
                Nutzungsbeginn = nutzungsbeginn,
                Nutzungsende = nutzungsende,
                Zeitanteil = zeitanteil,
                WFZeitanteil = gesamtWF > 0 ? zeitanteil * wohnung.VersionAt(abrechnungsende).Wohnflaeche / gesamtWF : 0,
                NFZeitanteil = gesamtNF > 0 ? zeitanteil * wohnung.VersionAt(abrechnungsende).Nutzflaeche / gesamtNF : 0,
                NEZeitanteil = gesamtNE > 0 ? zeitanteil * wohnung.VersionAt(abrechnungsende).Nutzeinheit / gesamtNE : 0,
                MEAZeitanteil = gesamtMEA > 0 ? zeitanteil * wohnung.VersionAt(abrechnungsende).Miteigentumsanteile / gesamtMEA : 0,
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
            DateOnly ende,
            int jahr,
            DateOnly abrechnungsEnde)
        {
            var result = new List<NkRechnungsplan>();

            foreach (var umlage in umlagen)
            {
                var hkvo = umlage.HkvoAt(abrechnungsEnde);
                if (hkvo != null)
                {
                    ProcessHkvoUmlage(umlage, hkvo, parteien, result, beginn, ende, jahr);
                    continue;
                }

                var schluessel = umlage.VersionAt(abrechnungsEnde).Schluessel;
                var verrechnungsZeilen = umlage.NkVerrechnungsKonto?.Buchungszeilen
                    .Where(z => z.Buchungssatz.Buchungsjahr == jahr
                             && IstRechnungsZeile(z))
                    .Select(z => (Zeile: z, Betrag: z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag))
                    .ToList() ?? [];

                foreach (var (zeile, betrag) in verrechnungsZeilen)
                {
                    if (betrag == 0) continue;
                    var warnungen = new List<string>();
                    var anteile = new List<NkRechnungsAnteil>();

                    foreach (var partei in parteien)
                    {
                        decimal anteilFaktor = partei.GetAnteil(schluessel, umlage.UmlageId);
                        if (anteilFaktor <= 0) continue;

                        decimal anteil = Math.Round(betrag * anteilFaktor, 2);
                        if (anteil == 0) continue;

                        anteile.Add(new NkRechnungsAnteil { Partei = partei, Betrag = anteil });
                    }

                    ApplyRundungskorrektur(betrag, anteile);

                    result.Add(new NkRechnungsplan
                    {
                        Buchungssatz = zeile.Buchungssatz,
                        Betrag = betrag,
                        Umlage = umlage,
                        Anteile = anteile,
                        Warnungen = warnungen
                    });
                }
            }

            ApplyStrompauschale(umlagen, parteien, result, jahr);

            return result;
        }

        /// <summary>Beschreibungs-Marker der Strompauschale-Umbuchung (HeizkostenV).</summary>
        public const string StrompauschaleMarker = "Strompauschale-Umbuchung";

        /// <summary>
        /// Beschreibungs-Marker manueller NK-Anteil-Sätze (Soll NkBuchungskonto /
        /// Haben NkVerrechnungsKonto). Wird von NkAnteilBuchungsService verwendet.
        /// </summary>
        public const string NkAnteilMarker = "NK-Anteil: ";

        /// <summary>
        /// Wendet die Strompauschale (HeizkostenV) an: delta = Heizkosten × Strompauschale wird
        /// vom Allgemeinstrom/Betriebsstrom abgezogen und auf die Heizkosten umgelegt.
        /// Die Anteile der Betriebsstrom-Pläne werden anteilig um delta gekürzt; zusätzlich
        /// entsteht ein Heizkosten-Plan über delta (verteilt nach den vorhandenen Heizkosten-
        /// Anteilen) mit einer Umbuchung als Buchungssatz (Soll Betriebsstrom-NkVK /
        /// Haben Heizkosten-NkVK). Heizkosten- und Betriebsstrom-Umlage müssen dieselben
        /// Wohnungen (= dieselbe Einheit) haben.
        /// </summary>
        private static void ApplyStrompauschale(
            List<Umlage> umlagen, List<NkPartei> parteien, List<NkRechnungsplan> plaene, int jahr)
        {
            var endeJahr = new DateOnly(jahr, 12, 31);
            var umlageIds = umlagen.Select(u => u.UmlageId).ToHashSet();

            foreach (var heiz in umlagen)
            {
                var hkvo = heiz.HkvoAt(endeJahr);
                if (hkvo?.Betriebsstrom == null || hkvo.Strompauschale <= 0) continue;

                var betrId = hkvo.Betriebsstrom.UmlageId;
                if (!umlageIds.Contains(betrId)) continue; // Betriebsstrom in anderer Einheit – hier nicht behandelt

                var heizPlaene = plaene.Where(p => p.Umlage.UmlageId == heiz.UmlageId && !p.IstStrompauschale).ToList();
                var heizTotal = heizPlaene.Sum(p => p.Betrag);
                if (heizTotal <= 0) continue;

                var delta = Math.Round(heizTotal * hkvo.Strompauschale, 2);
                if (delta <= 0) continue;

                var betrPlaene = plaene.Where(p => p.Umlage.UmlageId == betrId).ToList();
                var betrTotal = betrPlaene.Sum(p => p.Betrag);
                if (betrTotal <= 0)
                {
                    var dummy = new Buchungssatz(endeJahr, $"{StrompauschaleMarker} {heiz.Typ.Bezeichnung} {jahr}")
                    {
                        Buchungsjahr = jahr
                    };
                    plaene.Add(new NkRechnungsplan
                    {
                        Buchungssatz = dummy,
                        Betrag = 0,
                        Umlage = heiz,
                        Anteile = [],
                        Warnungen = [$"Strompauschale kann nicht angewandt werden: '{hkvo.Betriebsstrom.Typ.Bezeichnung}' hat keine Buchungen für {jahr}."],
                        IstStrompauschale = true
                    });
                    continue;
                }

                var warnungen = new List<string>();
                if (delta > betrTotal)
                {
                    warnungen.Add(
                        $"Strompauschale ({delta:N2} €) übersteigt die Allgemeinstromrechnung ({betrTotal:N2} €) — auf diese gekürzt.");
                    delta = betrTotal;
                }

                // Strompauschale fließt VOR der §7/§8-Aufteilung in die Heizkosten ein:
                // die Heizkosten-Pläne werden um delta hochskaliert (Betrag + Anteile),
                // die §7/§8-Proportionen bleiben erhalten. So bleibt es EINE Heizkosten-Zeile
                // (BK + Pauschale), kein separater Pauschal-Plan.
                var heizFaktor = (heizTotal + delta) / heizTotal;
                for (var i = 0; i < heizPlaene.Count; i++)
                {
                    var hp = heizPlaene[i];
                    var neuBetrag = i == heizPlaene.Count - 1
                        ? heizTotal + delta - heizPlaene.Take(i).Sum(p => Math.Round(p.Betrag * heizFaktor, 2))
                        : Math.Round(hp.Betrag * heizFaktor, 2);
                    var skaliert = hp.Anteile
                        .Select(a => a with { Betrag = Math.Round(a.Betrag * heizFaktor, 2) })
                        .ToList();
                    ApplyRundungskorrektur(neuBetrag, skaliert);
                    plaene[plaene.IndexOf(hp)] = CopyPlanMitAnteilen(hp, skaliert, neuBetrag);
                }

                // Betriebsstrom-Pläne anteilig um delta kürzen (Betrag UND Anteile),
                // damit der Abzug auch in der Vorschau/Druck sichtbar ist.
                var kuerzungsFaktor = 1m - delta / betrTotal;
                foreach (var bp in betrPlaene)
                {
                    var neuBetrag = Math.Round(bp.Betrag * kuerzungsFaktor, 2);
                    var gekuerzt = bp.Anteile
                        .Select(a => a with { Betrag = Math.Round(a.Betrag * kuerzungsFaktor, 2) })
                        .Where(a => a.Betrag > 0)
                        .ToList();
                    ApplyRundungskorrektur(neuBetrag, gekuerzt);
                    plaene[plaene.IndexOf(bp)] = CopyPlanMitAnteilen(bp, gekuerzt, neuBetrag);
                }

                // Reines Buchungs-Artefakt: balancierte Umbuchung (Soll Betriebsstrom-NkVK /
                // Haben Heizkosten-NkVK) OHNE Anteile. Wird beim Buchen persistiert und gleicht
                // die Verrechnungskonten aus; sie ist KEINE Verteilungs-/Anzeigezeile
                // (in BuildAbrechnungseinheitenInfos übersprungen).
                var umbuchung = new Buchungssatz(endeJahr, $"{StrompauschaleMarker} {heiz.Typ.Bezeichnung} {jahr}")
                {
                    Buchungsjahr = jahr
                };
                umbuchung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, delta)
                {
                    Buchungssatz = umbuchung,
                    Buchungskonto = hkvo.Betriebsstrom.NkVerrechnungsKonto
                });
                umbuchung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, delta)
                {
                    Buchungssatz = umbuchung,
                    Buchungskonto = heiz.NkVerrechnungsKonto
                });

                plaene.Add(new NkRechnungsplan
                {
                    Buchungssatz = umbuchung,
                    Betrag = delta,
                    Umlage = heiz,
                    Anteile = [],
                    Warnungen = warnungen,
                    IstStrompauschale = true
                });
            }
        }

        private static NkRechnungsplan CopyPlanMitAnteilen(
            NkRechnungsplan plan, List<NkRechnungsAnteil> anteile, decimal? betrag = null,
            List<string>? warnungen = null) =>
            new()
            {
                Buchungssatz = plan.Buchungssatz,
                Betrag = betrag ?? plan.Betrag,
                Umlage = plan.Umlage,
                Anteile = anteile,
                Warnungen = warnungen ?? plan.Warnungen,
                Para9_2 = plan.Para9_2,
                P9Details = plan.P9Details,
                GesamtWaerme = plan.GesamtWaerme,
                GesamtWW = plan.GesamtWW,
                HkvoVerbrauchAnteile = plan.HkvoVerbrauchAnteile,
                IstStrompauschale = plan.IstStrompauschale
            };

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
            HKVO hkvo,
            List<NkPartei> parteien,
            List<NkRechnungsplan> result,
            DateOnly beginn,
            DateOnly ende,
            int jahr)
        {
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
            var totalWW = wwZaehlerByPartei.Values.SelectMany(x => x).Sum(x => x.Item2);

            // §9(2): Q vom HKVO-Allgemeinzähler, V = Summe aller Wohnungs-WW-Zähler.
            // Anteil P = 2,5 × V × (Tw − 10) / Q.
            const decimal tw = 60m;
            decimal para9_2 = 0;
            NkP9Details? p9Details = null;
            if (hkvo.AllgemeinWaerme != null)
            {
                var qVerbrauch = new Verbrauch(hkvo.AllgemeinWaerme, beginn, ende, notes);
                var Q = qVerbrauch.Delta;
                var wwVerbrauche = wohnungWW
                    .Select(z => (Zaehler: z, Verbrauch: new Verbrauch(z, beginn, ende, notes).Delta))
                    .ToList();
                var V = wwVerbrauche.Sum(x => x.Verbrauch);

                if (Q <= 0)
                {
                    notes.Add($"Allgemein-Wärmezähler {hkvo.AllgemeinWaerme.Kennnummer} hat keinen Verbrauch " +
                        $"im Zeitraum {beginn:dd.MM.yyyy} - {ende:dd.MM.yyyy} — kein §9(2)-Warmwasseranteil, " +
                        "der gesamte Betrag wird als Heizung (§7) verteilt.", Severity.Warning);
                }
                else if (V <= 0)
                {
                    notes.Add("Kein Warmwasser-Verbrauch auf den Wohnungszählern gefunden — kein §9(2)-" +
                        "Warmwasseranteil, der gesamte Betrag wird als Heizung (§7) verteilt.", Severity.Warning);
                }
                else
                {
                    para9_2 = 2.5m * V * (tw - 10m) / Q;
                    if (para9_2 > 1)
                    {
                        notes.Add($"§9(2)-Warmwasseranteil liegt über 100 % ({para9_2:P1}) und wird auf 100 % " +
                            "begrenzt — V, Q und Einheiten prüfen.", Severity.Error);
                        para9_2 = 1m;
                    }
                }

                if (Q > 0 && totalWaerme > Q)
                {
                    notes.Add($"Summe der Wohnungs-Wärmezähler ({totalWaerme}) übersteigt den " +
                        $"Allgemeinzähler {hkvo.AllgemeinWaerme.Kennnummer} ({Q}).", Severity.Warning);
                }

                p9Details = new NkP9Details(
                    V, Q, tw,
                    hkvo.AllgemeinWaerme.Kennnummer,
                    qVerbrauch.Anfangsdatum, qVerbrauch.Enddatum,
                    wwVerbrauche);
            }

            // Individuelle Verbrauchsanteile und Zähler-Einzelwerte je Partei
            var hkvoVerbrauchAnteile = parteien.ToDictionary(
                p => p,
                p =>
                {
                    var heizGesamt = waermeZaehlerByPartei[p].Sum(x => x.Item2);
                    var wwGesamt = wwZaehlerByPartei[p].Sum(x => x.Item2);
                    return new NkHkvoParteiVerbrauch(
                        HeizAnteil: totalWaerme > 0 ? heizGesamt / totalWaerme : 0m,
                        WWAnteil: totalWW > 0 ? wwGesamt / totalWW : 0m,
                        HeizZaehler: waermeZaehlerByPartei[p],
                        WwZaehler: wwZaehlerByPartei[p]);
                });

            // Notes (fehlende/abweichende Zählerstände, §9(2)-Plausibilität) als
            // Warnungen an alle Pläne dieser Umlage hängen — vorher gingen sie verloren.
            var warnungen = notes
                .Distinct()
                .Select(n => n.Severity == Severity.Error ? $"Fehler: {n.Message}" : n.Message)
                .ToList();

            // Maßgeblich ist ausschließlich das Buchungsjahr (Wirtschaftsjahr) — das
            // Buchungsdatum darf davon abweichen (z.B. Vorjahresrechnung, die erst im
            // Folgejahr bezahlt wird) und würde die Rechnung sonst doppelt zuordnen.
            var verrechnungsZeilen = umlage.NkVerrechnungsKonto?.Buchungszeilen
                .Where(z => z.Buchungssatz.Buchungsjahr == jahr
                         && IstRechnungsZeile(z))
                .Select(z => (Zeile: z, Betrag: z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag))
                .ToList() ?? [];

            foreach (var (zeile, betrag) in verrechnungsZeilen)
            {
                if (betrag == 0) continue;
                var betragHZ = betrag * (1 - para9_2);
                var betragWW = betrag * para9_2;
                var anteile = new List<NkRechnungsAnteil>();

                foreach (var partei in parteien)
                {
                    var hkvoPartei = hkvoVerbrauchAnteile[partei];

                    // Verbrauchsunabhängiger Anteil (§7/§8) wird nach NUTZfläche verteilt.
                    decimal heizKostenAnteil = totalWaerme > 0
                        ? p7 * hkvoPartei.HeizAnteil + (1 - p7) * partei.NFZeitanteil
                        : partei.NFZeitanteil;

                    decimal wwKostenAnteil = totalWW > 0
                        ? p8 * hkvoPartei.WWAnteil + (1 - p8) * partei.NFZeitanteil
                        : partei.NFZeitanteil;

                    decimal meinBetrag = Math.Round(betragHZ * heizKostenAnteil + betragWW * wwKostenAnteil, 2);
                    if (meinBetrag == 0) continue;

                    anteile.Add(new NkRechnungsAnteil { Partei = partei, Betrag = meinBetrag });
                }

                ApplyRundungskorrektur(betrag, anteile);

                result.Add(new NkRechnungsplan
                {
                    Buchungssatz = zeile.Buchungssatz,
                    Betrag = betrag,
                    Umlage = umlage,
                    Anteile = anteile,
                    Warnungen = warnungen,
                    // Immer setzen (0, wenn kein §9(2)-Warmwasserzähler): markiert die Zeile
                    // als HKVO-Zeile. null bleibt den kalten Betriebskosten vorbehalten —
                    // sonst würde eine Heizkostenzeile ohne §9(2) im Frontend als kalt gelten
                    // und die §7/§8-Aufteilung nicht angezeigt.
                    Para9_2 = para9_2,
                    P9Details = p9Details,
                    GesamtWaerme = totalWaerme > 0 ? totalWaerme : null,
                    GesamtWW = totalWW > 0 ? totalWW : null,
                    HkvoVerbrauchAnteile = hkvoVerbrauchAnteile,
                });
            }
        }

        /// <summary>
        /// Rechnungs-/Gutschriftszeilen auf dem NkVerrechnungskonto sind zu verteilende
        /// Forderungen: Haben-Zeilen (Rechnungen) und Soll-Zeilen ohne OPOS-Ausgleich
        /// (Gutschriften, Stornos). Keine Forderungen sind:
        ///   - Zahlungs-Soll-Zeilen — sie tragen immer einen OffenerPostenAusgleich
        ///     zur Forderungs-Haben-Zeile (TransaktionBuchungsService),
        ///   - die Strompauschale-Umbuchung (wird als eigener transienter Plan erzeugt),
        ///   - manuelle NK-Anteil-Sätze (Haben NkVK ist dort die Verteil-Gegenseite).
        /// Der Ausgleichszustand des Satzes ist bewusst KEIN Kriterium: Nach dem Buchen
        /// der NK-Anteile ist der Forderungssatz ausgeglichen, muss aber im Preview
        /// weiterhin als (gebuchte) Rechnung erscheinen — sonst kippen die Ergebnisse
        /// nach dem Buchen und die Zeilen gelten fälschlich als fehlend. Ebenso ist
        /// Buchungssatz.Transaktion KEIN Kriterium: importierte Forderungssätze können
        /// eine Transaktion tragen.
        /// </summary>
        private static bool IstRechnungsZeile(Buchungszeile zeile) =>
            !zeile.Buchungssatz.Beschreibung.StartsWith(StrompauschaleMarker)
            && !zeile.Buchungssatz.Beschreibung.StartsWith(NkAnteilMarker)
            && (zeile.SollHaben == SollHaben.Haben || zeile.AlsSollZeile.Count == 0);

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
