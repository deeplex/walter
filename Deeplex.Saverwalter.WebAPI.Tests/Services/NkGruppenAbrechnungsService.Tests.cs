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
                KautionsKonto = new Buchungskonto("1002", "Kaution", BuchungskontoTyp.Passiv),
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
    }
}
