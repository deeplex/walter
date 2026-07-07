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
using FluentAssertions;
using Xunit;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class NkGruppenAbrechnungsServiceTests
    {
        private static int _wohnungIdCounter = 1;
        private static Wohnung MakeWohnung(string name, decimal wf = 100, decimal nf = 100,
            decimal mea = 100, int ne = 1)
        {
            var w = new Wohnung(name) { WohnungId = _wohnungIdCounter++ };
            w.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), wf, nf, mea, ne) { Wohnung = w });
            return w;
        }

        private static Umlage MakeUmlage(Umlageschluessel schluessel, string typName = "Test",
            Buchungskonto? nkKonto = null)
        {
            var u = new Umlage { Typ = new Umlagetyp(typName) };
            if (nkKonto != null) u.NkVerrechnungsKonto = nkKonto;
            u.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), schluessel) { Umlage = u });
            return u;
        }

        private static Vertrag MakeVertrag(
            Wohnung wohnung,
            int personenzahl = 1,
            DateOnly? start = null,
            DateOnly? end = null)
        {
            var vertrag = new Vertrag
            {
                Wohnung = wohnung,
                MietBuchungskonto = new Buchungskonto("1000", "Miete", BuchungskontoTyp.Ertrag),
                NkBuchungskonto = new Buchungskonto("1001", "NK", BuchungskontoTyp.Ertrag),
                BkAbrechnungsKonto = new Buchungskonto("1003", "NK-Abr", BuchungskontoTyp.Ertrag),
                ZahlungsKonto = new Buchungskonto("1004", "Zahlung", BuchungskontoTyp.Aktiv),
                MietminderungsKonto = new Buchungskonto("1005", "Minderung", BuchungskontoTyp.Aufwand),
            };
            if (end.HasValue) vertrag.Ende = end.Value;
            var version = new VertragVersion(start ?? new DateOnly(2000, 1, 1), 1000, personenzahl);
            version.Vertrag = vertrag;
            vertrag.Versionen.Add(version);
            wohnung.Vertraege.Add(vertrag);
            return vertrag;
        }

        // ── Zeitanteil ────────────────────────────────────────────────────────

        [Fact]
        public void FullYearVertrag_ZeitanteilIsOne()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche);
            umlage.Wohnungen.Add(wohnung);

            var einheiten = ComputeEinheiten([umlage], 2021);

            var partei = einheiten.Single().Parteien.Single(p => p.Vertrag != null);
            partei.Zeitanteil.Should().Be(1.0m);
            partei.WFZeitanteil.Should().Be(1.0m);
        }

        [Fact]
        public void PartialYearVertrag_CorrectNutzungsbeginnAndEigenanteil()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung, start: new DateOnly(2021, 7, 1));
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche);
            umlage.Wohnungen.Add(wohnung);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var parteien = einheiten.Single().Parteien;

            parteien.Should().HaveCount(2);
            var mieter = parteien.Single(p => p.Vertrag != null);
            var eigenanteil = parteien.Single(p => p.Vertrag == null);

            mieter.Nutzungsbeginn.Should().Be(new DateOnly(2021, 7, 1));
            eigenanteil.Nutzungsende.Should().Be(new DateOnly(2021, 6, 30));
            (mieter.WFZeitanteil + eigenanteil.WFZeitanteil).Should().BeApproximately(1.0m, 0.0001m);
        }

        // ── Anteil-Verteilung ─────────────────────────────────────────────────

        [Fact]
        public void TwoWohnungen_NachWohnflaeche_ProportionalToFlaeche()
        {
            var w1 = MakeWohnung("W1", wf: 100);
            var w2 = MakeWohnung("W2", wf: 200);
            MakeVertrag(w1); MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche);
            umlage.Wohnungen.AddRange([w1, w2]);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var p1 = einheiten.Single().Parteien.Single(p => p.Wohnung == w1);
            var p2 = einheiten.Single().Parteien.Single(p => p.Wohnung == w2);

            p1.WFZeitanteil.Should().BeApproximately(1m / 3, 0.0001m);
            p2.WFZeitanteil.Should().BeApproximately(2m / 3, 0.0001m);
        }

        [Fact]
        public void TwoWohnungen_NachNutzeinheit_ProportionalToNE()
        {
            var w1 = MakeWohnung("W1", ne: 1);
            var w2 = MakeWohnung("W2", ne: 3);
            MakeVertrag(w1); MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachNutzeinheit);
            umlage.Wohnungen.AddRange([w1, w2]);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var p1 = einheiten.Single().Parteien.Single(p => p.Wohnung == w1);
            var p2 = einheiten.Single().Parteien.Single(p => p.Wohnung == w2);

            p1.NEZeitanteil.Should().BeApproximately(1m / 4, 0.0001m);
            p2.NEZeitanteil.Should().BeApproximately(3m / 4, 0.0001m);
        }

        [Fact]
        public void NachPersonenzahl_ProportionalToPersonenzahl()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            MakeVertrag(w1, personenzahl: 2);
            MakeVertrag(w2, personenzahl: 3);
            var umlage = MakeUmlage(Umlageschluessel.NachPersonenzahl);
            umlage.Wohnungen.AddRange([w1, w2]);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var p1 = einheiten.Single().Parteien.Single(p => p.Wohnung == w1);
            var p2 = einheiten.Single().Parteien.Single(p => p.Wohnung == w2);

            p1.PersonenZeitanteile.Sum(a => a.Anteil).Should().BeApproximately(2m / 5, 0.0001m);
            p2.PersonenZeitanteile.Sum(a => a.Anteil).Should().BeApproximately(3m / 5, 0.0001m);
        }

        // ── Leerstand / Eigenanteil ───────────────────────────────────────────

        [Fact]
        public void NoVertrag_ProducesEigenanteilPartei()
        {
            var wohnung = MakeWohnung("W1");
            // No Vertrag added
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche);
            umlage.Wohnungen.Add(wohnung);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var partei = einheiten.Single().Parteien.Single();

            partei.Vertrag.Should().BeNull();
            partei.WFZeitanteil.Should().Be(1.0m);
        }

        private static void AddBkForderung(Umlage umlage, decimal betrag, DateOnly datum, int jahr)
        {
            var satz = new Buchungssatz(datum, $"BK-Forderung {umlage.Typ?.Bezeichnung} {jahr}") { Buchungsjahr = jahr };
            var zeile = new Buchungszeile(SollHaben.Haben, betrag) { Buchungssatz = satz, Buchungskonto = umlage.NkVerrechnungsKonto };
            satz.Buchungszeilen.Add(zeile);
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(zeile);
        }

        private static Zaehler ZaehlerMitStaenden(Zaehlertyp typ, Wohnung w,
            params (DateOnly d, decimal s)[] staende)
        {
            var z = new Zaehler("Z", typ) { Wohnung = w };
            foreach (var (d, s) in staende)
                z.Staende.Add(new Zaehlerstand(d, s) { Zaehler = z });
            return z;
        }

        // ── Mieterwechsel ─────────────────────────────────────────────────────

        [Fact]
        public void Verbrauch_ZaehlerExistierteImJahrNochNicht_KeinFehlenderStandFehler()
        {
            // Ganzjähriger Mieter 2020. Der Wasserzähler hat seinen ERSTEN Stand erst 2021
            // (nach Abrechnungsende) — existierte 2020 also noch nicht. Kein Fehler.
            var w1 = MakeWohnung("W1");
            MakeVertrag(w1);
            var umlage = MakeUmlage(Umlageschluessel.NachVerbrauch,
                nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(w1);
            umlage.Zaehler.Add(ZaehlerMitStaenden(Zaehlertyp.Kaltwasser, w1,
                (new DateOnly(2021, 3, 1), 0), (new DateOnly(2021, 12, 31), 50)));
            AddBkForderung(umlage, 100m, new DateOnly(2020, 12, 31), 2020);

            var plaene = ComputeEinheiten([umlage], 2020).Single().Rechnungsplaene
                .Where(p => !p.IstStrompauschale).ToList();

            plaene.SelectMany(p => p.Warnungen)
                .Should().NotContain(w => w.Contains("Zählerstand fehlt"));
        }

        [Fact]
        public void Verbrauch_FehlenderEinzugsstand_NachmieterRestBleibtBeimEigenanteil_UndFehler()
        {
            // W1: MieterA 01.01.–31.07. (Auszugsstand vorhanden), danach Leerstand,
            //     dann Nachmieter MieterB ab 15.10. OHNE Einzugsstand.
            // W2: MieterC ganzjährig.
            // W1-Zähler: Ganzjahr 73, MieterA (bis 31.07.) 44 → Rest 29 nicht messbar.
            // W2-Zähler: 130. Gesamt 203, Rechnung 203 € (1 €/m³).
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            var mieterA = MakeVertrag(w1, start: new DateOnly(2020, 1, 1), end: new DateOnly(2020, 7, 31));
            var mieterB = MakeVertrag(w1, start: new DateOnly(2020, 10, 15));
            MakeVertrag(w2);

            var umlage = MakeUmlage(Umlageschluessel.NachVerbrauch,
                nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.AddRange([w1, w2]);
            umlage.Zaehler.Add(ZaehlerMitStaenden(Zaehlertyp.Kaltwasser, w1,
                (new DateOnly(2019, 12, 31), 0), (new DateOnly(2020, 7, 31), 44), (new DateOnly(2020, 12, 31), 73)));
            umlage.Zaehler.Add(ZaehlerMitStaenden(Zaehlertyp.Kaltwasser, w2,
                (new DateOnly(2019, 12, 31), 0), (new DateOnly(2020, 12, 31), 130)));
            AddBkForderung(umlage, 203m, new DateOnly(2020, 12, 31), 2020);

            var einheit = ComputeEinheiten([umlage], 2020).Single();
            var plan = einheit.Rechnungsplaene.Single(p => !p.IstStrompauschale);

            decimal AnteilVon(Vertrag v) => plan.Anteile
                .Where(a => a.Partei.Vertrag == v).Sum(a => a.Betrag);

            // MieterA zahlt NUR seinen gemessenen Verbrauch (44), NICHT 44+29.
            AnteilVon(mieterA).Should().BeApproximately(44m, 0.5m);
            // Der nicht messbare Rest (29) landet beim Eigenanteil (Vertrag == null).
            plan.Anteile.Where(a => a.Partei.Vertrag is null).Sum(a => a.Betrag)
                .Should().BeApproximately(29m, 0.5m);
            // Summe bleibt vollständig verteilt.
            plan.Anteile.Sum(a => a.Betrag).Should().BeApproximately(203m, 0.02m);
            // Fehlender Einzugsstand für den Nachmieter wird als Fehler gemeldet.
            plan.Warnungen.Should().Contain(w => w.Contains("Fehler:"));
        }

        [Fact]
        public void Mieterwechsel_BetragWirdNachZeitanteilAufgeteilt()
        {
            var wohnung = MakeWohnung("W1");
            var vertragA = MakeVertrag(wohnung, start: new DateOnly(2021, 1, 1), end: new DateOnly(2021, 6, 30));
            var vertragB = MakeVertrag(wohnung, start: new DateOnly(2021, 7, 1));
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK Test", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var einheit = einheiten.Single();

            // Lückenlose Belegung → kein Eigenanteil
            einheit.Parteien.Where(p => p.Vertrag != null).Should().HaveCount(2);
            einheit.Parteien.Where(p => p.Vertrag == null).Should().BeEmpty();

            // Gesamtbetrag erhalten
            var plan = einheit.Rechnungsplaene.Single();
            plan.Anteile.Sum(a => a.Betrag).Should().Be(1000m);

            // Anteile proportional zu Nutzungstagen (181 / 184 bei 365 Tagen)
            var anteilA = plan.Anteile.Single(a => a.Partei.Vertrag == vertragA).Betrag;
            var anteilB = plan.Anteile.Single(a => a.Partei.Vertrag == vertragB).Betrag;
            anteilA.Should().BeApproximately(1000m * 181m / 365m, 0.01m);
            anteilB.Should().BeApproximately(1000m * 184m / 365m, 0.01m);
        }

        // ── NK-Anteil-Buchungssatz ────────────────────────────────────────────

        [Fact]
        public void NkAnteilBuchungssatz_WirdNichtAlsBkRechnungVerteilt()
        {
            var wohnung = MakeWohnung("W1");
            var vertrag = MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);

            // Bereits gebuchter manueller NK-Anteil: Soll NkBuchungskonto, Haben NkVerrechnungsKonto
            // (genau das, was NkAnteilBuchungsService.BucheVertragsNkAnteilAsync erzeugt)
            var nkAnteilSatz = new Buchungssatz(new DateOnly(2021, 6, 1), "NK-Anteil: Test 2021") { Buchungsjahr = 2021 };
            var nkHaben = new Buchungszeile(SollHaben.Haben, 400m) { Buchungssatz = nkAnteilSatz, Buchungskonto = umlage.NkVerrechnungsKonto };
            nkAnteilSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 400m) { Buchungssatz = nkAnteilSatz, Buchungskonto = vertrag.NkBuchungskonto });
            nkAnteilSatz.Buchungszeilen.Add(nkHaben);
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(nkHaben);

            var einheiten = ComputeEinheiten([umlage], 2021);

            // Erwartet: nur 1 Rechnungsplan (BK-Rechnung 1000 €), nicht 2
            einheiten.Single().Rechnungsplaene.Should().HaveCount(1);
            einheiten.Single().Rechnungsplaene.Single().Betrag.Should().Be(1000m);
        }

        [Fact]
        public void PartielleVerteilung_BuchungssatzWirdNochBeruecksichtigt()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            var v1 = MakeVertrag(w1);
            MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.AddRange([w1, w2]);

            // Forderungs-BS: 1000 € Haben, aber erst 500 € Soll (nur Vertrag 1 schon eingetragen)
            var satz = new Buchungssatz(new DateOnly(2021, 12, 31), "BK-Forderung Test 2021") { Buchungsjahr = 2021 };
            var haben = new Buchungszeile(SollHaben.Haben, 1000m) { Buchungssatz = satz, Buchungskonto = umlage.NkVerrechnungsKonto };
            satz.Buchungszeilen.Add(haben);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = satz, Buchungskonto = v1.NkBuchungskonto });
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(haben);

            var einheiten = ComputeEinheiten([umlage], 2021);

            // SollSumme(500) < HabenSumme(1000) → noch nicht vollständig verteilt, wird eingeschlossen
            einheiten.Single().Rechnungsplaene.Should().HaveCount(1);
            einheiten.Single().Rechnungsplaene.Single().Betrag.Should().Be(1000m);
        }

        [Fact]
        public void VollstaendigVerteilterBS_WirdIdentischNeuGeplant()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            var v1 = MakeVertrag(w1);
            var v2 = MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.AddRange([w1, w2]);

            // Forderungs-BS: 1000 € Haben, 500 + 500 € Soll → ausgeglichen (gebucht)
            var satz = new Buchungssatz(new DateOnly(2021, 12, 31), "BK-Forderung Test 2021") { Buchungsjahr = 2021 };
            var haben = new Buchungszeile(SollHaben.Haben, 1000m) { Buchungssatz = satz, Buchungskonto = umlage.NkVerrechnungsKonto };
            satz.Buchungszeilen.Add(haben);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = satz, Buchungskonto = v1.NkBuchungskonto });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 500m) { Buchungssatz = satz, Buchungskonto = v2.NkBuchungskonto });
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(haben);

            var einheiten = ComputeEinheiten([umlage], 2021);

            // Auch ein bereits vollständig gebuchter Satz wird geplant — mit denselben
            // Anteilen wie gebucht. Der Preview zeigt darüber geplant vs. gebucht;
            // erneutes Buchen ist idempotent (NkAnteilBuchungsService überspringt
            // bereits bebuchte Konten).
            var plan = einheiten.Single().Rechnungsplaene.Single();
            plan.Betrag.Should().Be(1000m);
            plan.Anteile.Single(a => a.Partei.Wohnung == w1).Betrag.Should().Be(500m);
            plan.Anteile.Single(a => a.Partei.Wohnung == w2).Betrag.Should().Be(500m);
        }

        // ── Gutschrift ────────────────────────────────────────────────────────

        private static void AddGutschrift(Umlage umlage, decimal betrag, DateOnly datum, int jahr)
        {
            var satz = new Buchungssatz(datum, $"Gutschrift {umlage.Typ?.Bezeichnung} {jahr}") { Buchungsjahr = jahr };
            var zeile = new Buchungszeile(SollHaben.Soll, Math.Abs(betrag)) { Buchungssatz = satz, Buchungskonto = umlage.NkVerrechnungsKonto };
            satz.Buchungszeilen.Add(zeile);
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(zeile);
        }

        [Fact]
        public void Gutschrift_WirdAlsNegativerBetragVerteilt()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddGutschrift(umlage, -600m, new DateOnly(2021, 6, 1), 2021);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var plan = einheiten.Single().Rechnungsplaene.Single();

            plan.Betrag.Should().Be(-600m);
            plan.Anteile.Single(a => a.Partei.Vertrag != null).Betrag.Should().Be(-600m);
        }

        [Fact]
        public void GutschriftUndRechnung_BeideWerdenVerteilt()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            MakeVertrag(w1); MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.AddRange([w1, w2]);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);
            AddGutschrift(umlage, -200m, new DateOnly(2021, 6, 1), 2021);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var plaene = einheiten.Single().Rechnungsplaene;

            plaene.Should().HaveCount(2);
            plaene.Sum(p => p.Betrag).Should().Be(800m);
            plaene.Sum(p => p.Anteile.Sum(a => a.Betrag)).Should().Be(800m);
        }

        // ── Preview nach Buchung / Zahlungen ──────────────────────────────────

        [Fact]
        public void GebuchterForderungssatz_BleibtImRechnungsplan()
        {
            // Nach dem Buchen der NK-Anteile ist der Forderungssatz ausgeglichen
            // (Haben NkVK + Soll Mieterkonto). Der Preview muss ihn weiterhin
            // planen, sonst kippt das Ergebnis nach dem Buchen.
            var wohnung = MakeWohnung("W1");
            var vertrag = MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche,
                nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);

            var satz = umlage.NkVerrechnungsKonto!.Buchungszeilen.Single().Buchungssatz;
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 1000m)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.NkBuchungskonto
            });

            var plan = ComputeEinheiten([umlage], 2021).Single().Rechnungsplaene.Single();

            plan.Betrag.Should().Be(1000m);
            plan.Anteile.Sum(a => a.Betrag).Should().Be(1000m);
        }

        [Fact]
        public void Zahlungssatz_WirdNichtAlsForderungGeplant()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche,
                nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);
            var forderungsHaben = umlage.NkVerrechnungsKonto!.Buchungszeilen
                .Single(z => z.SollHaben == SollHaben.Haben);

            // Zahlung an den Dienstleister: Soll NkVK / Haben Zahlungskonto,
            // mit OPOS-Ausgleich gegen die Forderungs-Haben-Zeile.
            var zahlungsSatz = new Buchungssatz(new DateOnly(2021, 12, 31), "Zahlung Betriebskosten Test 2021");
            var sollZeile = new Buchungszeile(SollHaben.Soll, 1000m)
            {
                Buchungssatz = zahlungsSatz,
                Buchungskonto = umlage.NkVerrechnungsKonto
            };
            sollZeile.AlsSollZeile.Add(new OffenerPostenAusgleich
            {
                SollZeile = sollZeile,
                HabenZeile = forderungsHaben
            });
            zahlungsSatz.Buchungszeilen.Add(sollZeile);
            zahlungsSatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            {
                Buchungssatz = zahlungsSatz,
                Buchungskonto = new Buchungskonto("1900", "Bank", BuchungskontoTyp.Aktiv)
            });
            umlage.NkVerrechnungsKonto!.Buchungszeilen.Add(sollZeile);

            var plaene = ComputeEinheiten([umlage], 2021).Single().Rechnungsplaene;

            plaene.Should().ContainSingle().Which.Betrag.Should().Be(1000m);
        }

        [Fact]
        public void StrompauschaleUmbuchung_WirdNichtAlsForderungGeplant()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche,
                nkKonto: new Buchungskonto("7000", "NK-VK", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 1000m, new DateOnly(2021, 12, 31), 2021);

            // Persistierte Strompauschale-Umbuchung: Haben auf diesem NkVK.
            var umbuchung = new Buchungssatz(new DateOnly(2021, 12, 31), $"{StrompauschaleMarker} Heizkosten 2021")
            {
                Buchungsjahr = 2021
            };
            var habenZeile = new Buchungszeile(SollHaben.Haben, 50m)
            {
                Buchungssatz = umbuchung,
                Buchungskonto = umlage.NkVerrechnungsKonto
            };
            umbuchung.Buchungszeilen.Add(habenZeile);
            umlage.NkVerrechnungsKonto!.Buchungszeilen.Add(habenZeile);

            var plaene = ComputeEinheiten([umlage], 2021).Single().Rechnungsplaene;

            plaene.Should().ContainSingle().Which.Betrag.Should().Be(1000m);
        }

        // ── Rechnungsplaene ───────────────────────────────────────────────────

        [Fact]
        public void RechnungsplanAnteile_SumEqualsRechnungBetrag()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            MakeVertrag(w1); MakeVertrag(w2);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK Test", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.AddRange([w1, w2]);
            AddBkForderung(umlage, 1001m, new DateOnly(2021, 12, 31), 2021);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var plan = einheiten.Single().Rechnungsplaene.Single();

            plan.Anteile.Sum(a => a.Betrag).Should().Be(1001m);
        }

        [Fact]
        public void SingleWohnungSingleRechnung_FullBetragGoesToPartei()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche, nkKonto: new Buchungskonto("7000", "NK-VK Test", BuchungskontoTyp.Passiv));
            umlage.Wohnungen.Add(wohnung);
            AddBkForderung(umlage, 500m, new DateOnly(2021, 12, 31), 2021);

            var einheiten = ComputeEinheiten([umlage], 2021);
            var plan = einheiten.Single().Rechnungsplaene.Single();
            var anteil = plan.Anteile.Single(a => a.Partei.Vertrag != null);

            anteil.Betrag.Should().Be(500m);
        }

        [Fact]
        public void NoRechnungen_RechnungsplaeneIsEmpty()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var umlage = MakeUmlage(Umlageschluessel.NachWohnflaeche);
            umlage.Wohnungen.Add(wohnung);

            var einheiten = ComputeEinheiten([umlage], 2021);

            einheiten.Single().Rechnungsplaene.Should().BeEmpty();
        }

        [Fact]
        public void TwoUmlagenSamWohnungen_GroupedIntoOneEinheit()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            MakeVertrag(w1); MakeVertrag(w2);
            var u1 = MakeUmlage(Umlageschluessel.NachWohnflaeche, "U1");
            var u2 = MakeUmlage(Umlageschluessel.NachNutzeinheit, "U2");
            u1.Wohnungen.AddRange([w1, w2]);
            u2.Wohnungen.AddRange([w1, w2]);

            var einheiten = ComputeEinheiten([u1, u2], 2021);

            einheiten.Should().HaveCount(1);
            einheiten.Single().Umlagen.Should().HaveCount(2);
        }

        [Fact]
        public void TwoUmlagenDifferentWohnungen_GroupedIntoTwoEinheiten()
        {
            var w1 = MakeWohnung("W1");
            var w2 = MakeWohnung("W2");
            MakeVertrag(w1); MakeVertrag(w2);
            var u1 = MakeUmlage(Umlageschluessel.NachWohnflaeche, "U1");
            var u2 = MakeUmlage(Umlageschluessel.NachWohnflaeche, "U2");
            u1.Wohnungen.Add(w1);
            u2.Wohnungen.Add(w2);

            var einheiten = ComputeEinheiten([u1, u2], 2021);

            einheiten.Should().HaveCount(2);
        }

        // ── Strompauschale (HeizkostenV) ──────────────────────────────────────

        private static (Umlage heiz, Umlage betr, Buchungskonto heizKonto, Buchungskonto betrKonto)
            MakeHkvoSetup(Wohnung wohnung, decimal heizBetrag, decimal betrBetrag, decimal strompauschale, int jahr)
        {
            var heizKonto = new Buchungskonto("2000", "Heiz-NkVK", BuchungskontoTyp.Passiv);
            var betrKonto = new Buchungskonto("2001", "Betr-NkVK", BuchungskontoTyp.Passiv);

            var betr = MakeUmlage(Umlageschluessel.NachWohnflaeche, "Allgemeinstrom", betrKonto);
            betr.UmlageId = 9002;
            betr.Wohnungen.Add(wohnung);
            AddBkForderung(betr, betrBetrag, new DateOnly(jahr, 12, 31), jahr);

            var heiz = MakeUmlage(Umlageschluessel.NachVerbrauch, "Heizkosten", heizKonto);
            heiz.UmlageId = 9001;
            heiz.Wohnungen.Add(wohnung);
            AddBkForderung(heiz, heizBetrag, new DateOnly(jahr, 12, 31), jahr);
            heiz.HeizkostenHKVOs.Add(new HKVO(new DateOnly(2000, 1, 1), 0.7m, 0.7m, HKVO_P9A2.Satz_2, strompauschale)
            {
                Heizkosten = heiz,
                Betriebsstrom = betr
            });

            return (heiz, betr, heizKonto, betrKonto);
        }

        [Fact]
        public void Strompauschale_VerschiebtAnteilVonBetriebsstromZuHeizkosten()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var (heiz, betr, heizKonto, betrKonto) = MakeHkvoSetup(wohnung, heizBetrag: 2000m, betrBetrag: 1000m, strompauschale: 0.05m, jahr: 2021);

            var plaene = ComputeEinheiten([heiz, betr], 2021).SelectMany(e => e.Rechnungsplaene).ToList();

            const decimal delta = 100m; // 2000 × 5%

            plaene.Where(p => p.Umlage == heiz).Sum(p => p.Anteile.Sum(a => a.Betrag))
                .Should().BeApproximately(2000m + delta, 0.05m);
            plaene.Where(p => p.Umlage == betr).Sum(p => p.Anteile.Sum(a => a.Betrag))
                .Should().BeApproximately(1000m - delta, 0.05m);

            var sp = plaene.Single(p => p.IstStrompauschale);
            sp.Betrag.Should().BeApproximately(delta, 0.05m);
            sp.Buchungssatz.Buchungszeilen.Should().Contain(z =>
                z.SollHaben == SollHaben.Soll && z.Buchungskonto == betrKonto && z.Betrag == delta);
            sp.Buchungssatz.Buchungszeilen.Should().Contain(z =>
                z.SollHaben == SollHaben.Haben && z.Buchungskonto == heizKonto && z.Betrag == delta);
        }

        [Fact]
        public void Strompauschale_GesamtkostenBleibenErhalten()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var (heiz, betr, _, _) = MakeHkvoSetup(wohnung, heizBetrag: 2000m, betrBetrag: 1000m, strompauschale: 0.05m, jahr: 2021);

            var plaene = ComputeEinheiten([heiz, betr], 2021).SelectMany(e => e.Rechnungsplaene).ToList();

            // Summe aller verteilten Anteile bleibt = 2000 + 1000 (nur Umverteilung).
            plaene.Sum(p => p.Anteile.Sum(a => a.Betrag)).Should().BeApproximately(3000m, 0.05m);
        }

        [Fact]
        public void Strompauschale_WirdAufAllgemeinstromGekuerzt_WennGroesser()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            // delta = 2000 × 50% = 1000 > Allgemeinstrom 200 → auf 200 gekürzt.
            var (heiz, betr, _, _) = MakeHkvoSetup(wohnung, heizBetrag: 2000m, betrBetrag: 200m, strompauschale: 0.5m, jahr: 2021);

            var plaene = ComputeEinheiten([heiz, betr], 2021).SelectMany(e => e.Rechnungsplaene).ToList();

            plaene.Where(p => p.Umlage == betr).Sum(p => p.Anteile.Sum(a => a.Betrag))
                .Should().BeApproximately(0m, 0.05m);
            plaene.Single(p => p.IstStrompauschale).Betrag.Should().BeApproximately(200m, 0.05m);
            plaene.Single(p => p.IstStrompauschale).Warnungen.Should().NotBeEmpty();
        }

        [Fact]
        public void Strompauschale_IstInDieHeizkostenZeileEingerechnet()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var (heiz, betr, _, _) = MakeHkvoSetup(wohnung, heizBetrag: 2000m, betrBetrag: 1000m, strompauschale: 0.05m, jahr: 2021);

            var plaene = ComputeEinheiten([heiz, betr], 2021).SelectMany(e => e.Rechnungsplaene).ToList();

            // Die Heizkosten-Verteilzeile trägt BK + delta (= 2100), kein separater Pauschal-Plan.
            var heizZeile = plaene.Single(p => p.Umlage == heiz && !p.IstStrompauschale);
            heizZeile.Betrag.Should().BeApproximately(2100m, 0.05m);
            heizZeile.Anteile.Sum(a => a.Betrag).Should().BeApproximately(2100m, 0.05m);

            // Die Umbuchung ist ein reines Buchungs-Artefakt ohne Anteile.
            plaene.Single(p => p.IstStrompauschale).Anteile.Should().BeEmpty();
        }

        [Fact]
        public void Hkvo_VerbrauchsunabhaengigerAnteil_NachNutzflaeche()
        {
            // W1: WF 100 / NF 200, W2: WF 100 / NF 100. Heizkosten ohne Verbrauchszähler
            // → verbrauchsunabhängig → Verteilung nach NUTZfläche (2:1), nicht Wohnfläche (1:1).
            var w1 = MakeWohnung("W1", wf: 100, nf: 200);
            var w2 = MakeWohnung("W2", wf: 100, nf: 100);
            MakeVertrag(w1);
            MakeVertrag(w2);

            var heiz = MakeUmlage(Umlageschluessel.NachVerbrauch, "Heizkosten",
                new Buchungskonto("2000", "Heiz-NkVK", BuchungskontoTyp.Passiv));
            heiz.UmlageId = 9101;
            heiz.Wohnungen.AddRange([w1, w2]);
            AddBkForderung(heiz, 900m, new DateOnly(2021, 12, 31), 2021);
            heiz.HeizkostenHKVOs.Add(new HKVO(new DateOnly(2000, 1, 1), 0.5m, 0.5m, HKVO_P9A2.Satz_2, 0m));

            var plan = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene.Single();
            var a1 = plan.Anteile.Single(a => a.Partei.Wohnung == w1).Betrag;
            var a2 = plan.Anteile.Single(a => a.Partei.Wohnung == w2).Betrag;

            a1.Should().BeApproximately(600m, 0.05m); // NF 200/300 × 900
            a2.Should().BeApproximately(300m, 0.05m); // NF 100/300 × 900
        }

        // ── §9(2) Warmwasseranteil ────────────────────────────────────────────

        private static Zaehler MakeZaehler(
            string kennnummer, Zaehlertyp typ, int jahr, decimal delta, Wohnung? wohnung = null)
        {
            var zaehler = new Zaehler(kennnummer, typ) { Wohnung = wohnung };
            zaehler.Staende.Add(new Zaehlerstand(new DateOnly(jahr - 1, 12, 31), 0) { Zaehler = zaehler });
            zaehler.Staende.Add(new Zaehlerstand(new DateOnly(jahr, 12, 31), delta) { Zaehler = zaehler });
            return zaehler;
        }

        private static Umlage MakeP9Setup(Wohnung wohnung, decimal q, decimal v, int jahr)
        {
            var heiz = MakeUmlage(Umlageschluessel.NachVerbrauch, "Heizkosten",
                new Buchungskonto("2000", "Heiz-NkVK", BuchungskontoTyp.Passiv));
            heiz.UmlageId = 9201;
            heiz.Wohnungen.Add(wohnung);
            AddBkForderung(heiz, 1000m, new DateOnly(jahr, 12, 31), jahr);

            heiz.Zaehler.Add(MakeZaehler("WW-1", Zaehlertyp.Warmwasser, jahr, v, wohnung));
            var hkvo = new HKVO(new DateOnly(2000, 1, 1), 0.7m, 0.7m, HKVO_P9A2.Satz_2, 0m)
            {
                Heizkosten = heiz,
                Betriebsstrom = MakeUmlage(Umlageschluessel.NachWohnflaeche, "Allgemeinstrom",
                    new Buchungskonto("2001", "Betr-NkVK", BuchungskontoTyp.Passiv)),
            };
            hkvo.AllgemeinWaermeZaehler.Add(MakeZaehler("ALLG-1", Zaehlertyp.Gas, jahr, q));
            heiz.HeizkostenHKVOs.Add(hkvo);
            return heiz;
        }

        [Fact]
        public void P9_2_EntsprichtSatz2Formel_MitDetails()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            // P = 2,5 × 100 × (60 − 10) / 25000 = 0,5
            var heiz = MakeP9Setup(wohnung, q: 25000m, v: 100m, jahr: 2021);

            var plan = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene.Single();

            plan.Para9_2.Should().Be(0.5m);
            plan.P9Details.Should().NotBeNull();
            plan.P9Details!.V.Should().Be(100m);
            plan.P9Details.Q.Should().Be(25000m);
            plan.P9Details.AllgemeinZaehlerKennnummer.Should().Be("ALLG-1");
            plan.P9Details.QAnfangsdatum.Should().Be(new DateOnly(2020, 12, 31));
            plan.P9Details.QEnddatum.Should().Be(new DateOnly(2021, 12, 31));
            plan.P9Details.WwZaehler.Should().ContainSingle(x => x.Verbrauch == 100m);
        }

        [Fact]
        public void P9_2_Q_SummiertMehrereAllgemeinzaehler()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            // Zwei Allgemein-Wärmezähler: Q = 15000 + 10000 = 25000.
            // P = 2,5 × 100 × (60 − 10) / 25000 = 0,5.
            var heiz = MakeP9Setup(wohnung, q: 15000m, v: 100m, jahr: 2021);
            heiz.HeizkostenHKVOs.Single().AllgemeinWaermeZaehler
                .Add(MakeZaehler("ALLG-2", Zaehlertyp.Gas, 2021, 10000m));

            var plan = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene.Single();

            plan.P9Details!.Q.Should().Be(25000m);
            plan.Para9_2.Should().Be(0.5m);
            plan.P9Details.AllgemeinZaehlerKennnummer.Should().Contain("ALLG-1").And.Contain("ALLG-2");
        }

        [Fact]
        public void P9_2_Ueber100Prozent_WirdBegrenztUndGewarnt()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            // P = 2,5 × 100 × 50 / 10000 = 1,25 → auf 1 begrenzt + Fehler-Warnung
            var heiz = MakeP9Setup(wohnung, q: 10000m, v: 100m, jahr: 2021);

            var plan = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene.Single();

            plan.Para9_2.Should().Be(1m);
            plan.Warnungen.Should().Contain(w => w.Contains("100 %"));
        }

        [Fact]
        public void HkvoUmlage_VorjahresrechnungMitZahldatumImJahr_WirdNichtGeplant()
        {
            // Rechnung für 2020, aber erst 2021 gebucht/bezahlt (Buchungsdatum 2021):
            // maßgeblich ist das Buchungsjahr — sie gehört NICHT in die 2021er Abrechnung.
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var heiz = MakeP9Setup(wohnung, q: 25000m, v: 100m, jahr: 2021);

            var vorjahr = new Buchungssatz(new DateOnly(2021, 2, 3), "Betriebskosten Heizkosten 2020")
            {
                Buchungsjahr = 2020
            };
            var zeile = new Buchungszeile(SollHaben.Haben, 500m)
            {
                Buchungssatz = vorjahr,
                Buchungskonto = heiz.NkVerrechnungsKonto
            };
            vorjahr.Buchungszeilen.Add(zeile);
            heiz.NkVerrechnungsKonto!.Buchungszeilen.Add(zeile);

            var plaene = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene;

            plaene.Should().ContainSingle().Which.Betrag.Should().Be(1000m);
        }

        [Fact]
        public void P9_2_OhneAllgemeinzaehlerVerbrauch_NullMitWarnung()
        {
            var wohnung = MakeWohnung("W1");
            MakeVertrag(wohnung);
            var heiz = MakeP9Setup(wohnung, q: 0m, v: 100m, jahr: 2021);

            var plan = ComputeEinheiten([heiz], 2021).Single().Rechnungsplaene.Single();

            plan.Para9_2.Should().Be(0m);
            plan.Warnungen.Should().Contain(w => w.Contains("ALLG-1"));
        }
    }

    public class VerbrauchTests
    {
        private static Zaehler MakeZaehlerMitStaenden(params (DateOnly Datum, decimal Stand)[] staende)
        {
            var zaehler = new Zaehler("Z-1", Zaehlertyp.Gas);
            foreach (var (datum, stand) in staende)
            {
                zaehler.Staende.Add(new Zaehlerstand(datum, stand) { Zaehler = zaehler });
            }
            return zaehler;
        }

        [Fact]
        public void Anfangsstand_NimmtDenStandAmNaechstenZumBeginn()
        {
            // Stände am 20.12. UND 31.12.: der 31.12. ist der richtige Anfangsstand
            // für den 01.01. — sonst zählen 11 Tage des Vorjahres mit.
            var zaehler = MakeZaehlerMitStaenden(
                (new DateOnly(2020, 12, 20), 100m),
                (new DateOnly(2020, 12, 31), 110m),
                (new DateOnly(2021, 12, 31), 210m));
            var notes = new List<Note>();

            var verbrauch = new Verbrauch(zaehler, new DateOnly(2021, 1, 1), new DateOnly(2021, 12, 31), notes);

            verbrauch.Delta.Should().Be(100m);
            verbrauch.Anfangsdatum.Should().Be(new DateOnly(2020, 12, 31));
            notes.Should().BeEmpty();
        }

        [Fact]
        public void Anfangsstand_KurzNachBeginn_WirdGenommen()
        {
            var zaehler = MakeZaehlerMitStaenden(
                (new DateOnly(2021, 1, 5), 100m),
                (new DateOnly(2021, 12, 28), 200m));
            var notes = new List<Note>();

            var verbrauch = new Verbrauch(zaehler, new DateOnly(2021, 1, 1), new DateOnly(2021, 12, 31), notes);

            verbrauch.Delta.Should().Be(100m);
            notes.Should().BeEmpty();
        }

        [Fact]
        public void AbweichendesMessfenster_ErzeugtWarnung()
        {
            // Erster Stand erst im März: Delta deckt nur ein Teiljahr ab → Warnung.
            var zaehler = MakeZaehlerMitStaenden(
                (new DateOnly(2021, 3, 1), 0m),
                (new DateOnly(2021, 12, 31), 50m));
            var notes = new List<Note>();

            var verbrauch = new Verbrauch(zaehler, new DateOnly(2021, 1, 1), new DateOnly(2021, 12, 31), notes);

            verbrauch.Delta.Should().Be(50m);
            notes.Should().Contain(n => n.Severity == Severity.Warning && n.Message.Contains("Messfenster"));
        }

        [Fact]
        public void FehlenderAnfangsstand_ErzeugtFehler()
        {
            var zaehler = MakeZaehlerMitStaenden((new DateOnly(2020, 6, 1), 100m));
            var notes = new List<Note>();

            var verbrauch = new Verbrauch(zaehler, new DateOnly(2021, 1, 1), new DateOnly(2021, 12, 31), notes);

            verbrauch.Delta.Should().Be(0m);
            notes.Should().Contain(n => n.Severity == Severity.Error);
        }
    }
}
