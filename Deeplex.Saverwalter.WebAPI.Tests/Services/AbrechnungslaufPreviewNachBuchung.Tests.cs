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
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// Preview und Buchen müssen unabhängig sein: Nach dem Buchen liefert der
    /// Preview dieselben Beträge wie davor — die Zeilen gelten dann als gebucht,
    /// nicht als fehlend (Regression aus dem IstVollstaendigVerteilt-Filter).
    /// </summary>
    public class AbrechnungslaufPreviewNachBuchungTests
    {
        private const int Jahr = 2021;

        private static AbrechnungslaufService Service(SaverwalterContext ctx) =>
            new(ctx,
                new NkAnteilBuchungsService(ctx),
                new AbrechnungsresultatBuchungsService(ctx),
                new AbrechnungsgruppenService(ctx),
                new StornoBuchungsService(ctx));

        private static void AddRechnung(SaverwalterContext ctx, Umlage umlage, decimal betrag)
        {
            var satz = new Buchungssatz(new DateOnly(Jahr, 6, 1), $"Betriebskosten {umlage.Typ.Bezeichnung} {Jahr}")
            {
                Buchungsjahr = Jahr
            };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });
            ctx.Buchungssaetze.Add(satz);
        }

        private static Umlage MitVersion(Umlage umlage, Umlageschluessel schluessel)
        {
            umlage.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), schluessel) { Umlage = umlage });
            return umlage;
        }

        private static (Vertrag vertrag, Umlage kalt, Umlage heiz) Seed(SaverwalterContext ctx)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);

            var kalt = new Umlage
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7001", "NK-VK Grundsteuer", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1201", "Zahlung Grundsteuer", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(MitVersion(kalt, Umlageschluessel.NachWohnflaeche));
            AddRechnung(ctx, kalt, 1000m);

            var betriebsstrom = new Umlage
            {
                Typ = new Umlagetyp("Allgemeinstrom"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7002", "NK-VK Strom", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1202", "Zahlung Strom", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(MitVersion(betriebsstrom, Umlageschluessel.NachWohnflaeche));
            AddRechnung(ctx, betriebsstrom, 200m);

            var heiz = new Umlage
            {
                Typ = new Umlagetyp("Heizkosten"),
                Wohnungen = [vertrag.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7003", "NK-VK Heizung", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1203", "Zahlung Heizung", BuchungskontoTyp.Aktiv),
            };
            heiz.HeizkostenHKVOs.Add(new HKVO(new DateOnly(2000, 1, 1), 0.7m, 0.7m, HKVO_P9A2.Satz_2, 0.05m)
            {
                Heizkosten = heiz,
                Betriebsstrom = betriebsstrom
            });
            ctx.Umlagen.Add(MitVersion(heiz, Umlageschluessel.NachVerbrauch));
            AddRechnung(ctx, heiz, 2000m);

            ctx.SaveChanges();
            return (vertrag, kalt, heiz);
        }

        [Fact]
        public async Task Preview_IstNachDemBuchenUnveraendert()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, kalt, heiz) = Seed(ctx);
            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            var vorher = await service.PreviewAsync(wohnungIds, Jahr);
            var vorherZeilen = vorher.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .ToDictionary(z => z.Bezeichnung, z => z.Betrag);

            await service.BookAsync(wohnungIds, Jahr);
            var nachher = await service.PreviewAsync(wohnungIds, Jahr);

            var nachherZeilen = nachher.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .ToList();

            // Keine Zeile verschwindet oder gilt als fehlend
            nachherZeilen.Should().HaveCount(vorherZeilen.Count);
            nachherZeilen.Should().OnlyContain(z => !z.IstFehlend);
            foreach (var zeile in nachherZeilen)
            {
                zeile.Betrag.Should().Be(vorherZeilen[zeile.Bezeichnung],
                    $"Betrag von '{zeile.Bezeichnung}' darf sich durch das Buchen nicht ändern");
                zeile.IstVollstaendigGebucht.Should().BeTrue(
                    $"'{zeile.Bezeichnung}' wurde gebucht");
            }

            // Rechnungsbeträge der Resultate bleiben gleich
            nachher.Resultate.Single().Rechnungsbetrag
                .Should().Be(vorher.Resultate.Single().Rechnungsbetrag);
        }

        [Fact]
        public async Task PersonenZeitanteile_KommenAusDerGroesstenEinheit()
        {
            // Vertrag 1 hat zusätzlich eine Einzel-Wohnungs-Umlage (eigene Einheit).
            // Die Personen-Intervalle im Resultat müssen trotzdem den Kontext der
            // großen Einheit zeigen (x/alle), nicht "x/x" aus der Einzel-Einheit.
            var ctx = TestUtils.GetContext();
            var vertrag1 = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var vertrag2 = TestUtils.FillVertragWithSomeData(ctx, 500m);
            vertrag1.Versionen.Single().Personenzahl = 2;
            vertrag2.Versionen.Single().Personenzahl = 3;

            var muell = new Umlage
            {
                Typ = new Umlagetyp("Müllbeseitigung"),
                Wohnungen = [vertrag1.Wohnung, vertrag2.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7010", "NK-VK Müll", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1210", "Zahlung Müll", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(MitVersion(muell, Umlageschluessel.NachPersonenzahl));
            AddRechnung(ctx, muell, 500m);

            var grundsteuer = new Umlage
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = [vertrag1.Wohnung],
                NkVerrechnungsKonto = new Buchungskonto("7011", "NK-VK Grundsteuer", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1211", "Zahlung Grundsteuer", BuchungskontoTyp.Aktiv),
            };
            ctx.Umlagen.Add(MitVersion(grundsteuer, Umlageschluessel.NachWohnflaeche));
            AddRechnung(ctx, grundsteuer, 300m);

            ctx.SaveChanges();
            var service = Service(ctx);

            var preview = await service.PreviewAsync(
                [vertrag1.Wohnung.WohnungId, vertrag2.Wohnung.WohnungId], Jahr);

            var resultat1 = preview.Resultate.Single(r =>
                r.VertragId == vertrag1.VertragId);
            resultat1.PersonenZeitanteile.Should().OnlyContain(p =>
                p.Personenzahl == 2 && p.GesamtPersonenzahl == 5);

            // Und die Verteilung der Personen-Umlage stimmt: 2/5 bzw. 3/5 von 500 €.
            var muellZeile = preview.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .Single(z => z.Bezeichnung == "Müllbeseitigung");
            muellZeile.Anteile.Single(a => a.VertragId == vertrag1.VertragId)
                .GeplanterBetrag.Should().Be(200m);
            muellZeile.Anteile.Single(a => a.VertragId == vertrag2.VertragId)
                .GeplanterBetrag.Should().Be(300m);
        }

        [Fact]
        public async Task Zahlung_MitOposAusgleich_WirdNichtAlsForderungGeplant()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, kalt, _) = Seed(ctx);
            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            // Zahlung an den Dienstleister: Soll NkVK / Haben Zahlungskonto,
            // mit OPOS-Ausgleich gegen die Forderungs-Haben-Zeile.
            var forderungsHaben = kalt.NkVerrechnungsKonto!.Buchungszeilen
                .Single(z => z.SollHaben == SollHaben.Haben);
            var zahlung = new Buchungssatz(new DateOnly(Jahr, 7, 1), "Zahlung Betriebskosten Grundsteuer")
            {
                Buchungsjahr = Jahr
            };
            var sollZeile = new Buchungszeile(SollHaben.Soll, 1000m)
            {
                Buchungssatz = zahlung,
                Buchungskonto = kalt.NkVerrechnungsKonto
            };
            zahlung.Buchungszeilen.Add(sollZeile);
            zahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 1000m)
            {
                Buchungssatz = zahlung,
                Buchungskonto = kalt.ZahlungsKonto
            });
            ctx.Buchungssaetze.Add(zahlung);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = sollZeile,
                HabenZeile = forderungsHaben
            });
            ctx.SaveChanges();

            var preview = await service.PreviewAsync(wohnungIds, Jahr);

            var grundsteuer = preview.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .Single(z => z.Bezeichnung == "Grundsteuer");
            grundsteuer.Betrag.Should().Be(1000m, "die Zahlung darf die Forderung nicht mindern");
        }

        [Fact]
        public async Task Buchen_VerteiltKalteUndWarmeAnteile()
        {
            var ctx = TestUtils.GetContext();
            var (vertrag, kalt, heiz) = Seed(ctx);
            var service = Service(ctx);
            var wohnungIds = new List<int> { vertrag.Wohnung.WohnungId };

            var result = await service.BookAsync(wohnungIds, Jahr);

            // Kalte Umlage: Forderungssatz trägt jetzt den Mieter-Soll-Anteil (voll, 1 Vertrag)
            var kaltSatz = kalt.NkVerrechnungsKonto!.Buchungszeilen
                .Select(z => z.Buchungssatz)
                .First(s => s.Beschreibung.StartsWith("Betriebskosten Grundsteuer"));
            kaltSatz.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungskonto == vertrag.NkBuchungskonto)
                .Sum(z => z.Betrag)
                .Should().Be(1000m, "der kalte NK-Anteil muss verteilt sein");

            // Warme Umlage: verteilt inkl. Strompauschale-Aufschlag (2000 + 5% = 2100)
            var heizSatz = heiz.NkVerrechnungsKonto!.Buchungszeilen
                .Select(z => z.Buchungssatz)
                .First(s => s.Beschreibung.StartsWith("Betriebskosten Heizkosten"));
            heizSatz.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungskonto == vertrag.NkBuchungskonto)
                .Sum(z => z.Betrag)
                .Should().Be(2100m, "der warme NK-Anteil inkl. Strompauschale muss verteilt sein");

            // Zweites Buchen ist idempotent (keine Konflikte, keine Doppelbuchung)
            await service.BookAsync(wohnungIds, Jahr);
            kaltSatz.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Soll).Should().HaveCount(1);
        }
    }
}
