// Copyright (c) 2023-2024 Kai Lawrence
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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Deeplex.Saverwalter.ModelTests
{
    public class TestUtils
    {
        public static Vertrag GetVertragForAbrechnung(SaverwalterContext ctx)
        {
            // Arrange
            var vertrag = FillVertragWithSomeData(ctx, 0);
            var vertrag2 = FillVertragWithSomeData(ctx, 0);
            ctx.Umlagen.AddRange(Add6Umlagen(new List<Wohnung>() { vertrag.Wohnung, vertrag2.Wohnung }));

            ctx.SaveChanges();

            return vertrag;
        }
        public static SaverwalterContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}");
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            var ctx = new SaverwalterContext(optionsBuilder.Options);

            return ctx;
        }
        public static Vertrag FillVertragWithSomeData(SaverwalterContext ctx, decimal grundmiete)
        {
            var vermieter = new Kontakt("TestKopf", Rechtsform.natuerlich)
            {
                Anrede = Anrede.Frau,
                Vorname = "TestVorname",
                Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt")
            };

            ctx.Kontakte.Add(vermieter);
            ctx.SaveChanges();

            var wohnung = new Wohnung("TestWohnung")
            {
                Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt"),
            };
            wohnung.Eigentuemer.Add(new WohnungEigentuemer(new DateOnly(2000, 1, 1)) { Wohnung = wohnung, Kontakt = vermieter });
            wohnung.Versionen.Add(new WohnungVersion(new DateOnly(2000, 1, 1), 100, 100, 100, 1) { Wohnung = wohnung });

            var vertrag = new Vertrag()
            {
                Ansprechpartner = vermieter,
                Wohnung = wohnung,
                MietBuchungskonto = new Buchungskonto("1000", "Miete", BuchungskontoTyp.Ertrag),
                NkBuchungskonto = new Buchungskonto("1001", "Nebenkosten-Vorauszahlung", BuchungskontoTyp.Ertrag),
                BkAbrechnungsKonto = new Buchungskonto("1003", "NK-Abrechnung", BuchungskontoTyp.Ertrag),
                ZahlungsKonto = new Buchungskonto("1004", "Zahlung", BuchungskontoTyp.Aktiv),
                MietminderungsKonto = new Buchungskonto("1005", "Mietminderung", BuchungskontoTyp.Aufwand),
            };
            var mieter = new Kontakt("TestMieter", Rechtsform.natuerlich)
            {
                Anrede = Anrede.Herr,
                Vorname = "TestVorname",
                Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt")
            };
            ctx.Kontakte.Add(mieter);
            mieter.Mietvertraege.Add(vertrag);

            ctx.SaveChanges();

            var version = new VertragVersion(
                new DateOnly(2020, 1, 1),
                grundmiete,
                1
            )
            {
                Vertrag = vertrag
            };
            ctx.VertragVersionen.Add(version);

            return vertrag;
        }

        private static void AddBkForderung(Umlage umlage, decimal betrag, DateOnly datum, int jahr)
        {
            var satz = new Buchungssatz(datum, $"BK-Forderung {umlage.Typ?.Bezeichnung} {jahr}")
            {
                Buchungsjahr = jahr
            };
            var zeile = new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = umlage.NkVerrechnungsKonto
            };
            satz.Buchungszeilen.Add(zeile);
            umlage.NkVerrechnungsKonto.Buchungszeilen.Add(zeile);
        }

        public static List<Umlage> Add6Umlagen(List<Wohnung> wohnungen)
        {
            var umlagen = new List<Umlage>();
            var date = new DateOnly(2021, 1, 1);

            var grundsteuer = new Umlage
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = wohnungen,
                NkVerrechnungsKonto = new Buchungskonto("7001", "NK-VK Grundsteuer", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1201", "Zahlung Grundsteuer", BuchungskontoTyp.Aktiv),
            };
            grundsteuer.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachWohnflaeche) { Umlage = grundsteuer });
            AddBkForderung(grundsteuer, 1000, date, 2021);
            umlagen.Add(grundsteuer);

            var dachrinnenreinigung = new Umlage
            {
                Typ = new Umlagetyp("Dachrinnenreinigung"),
                Wohnungen = wohnungen,
                NkVerrechnungsKonto = new Buchungskonto("7002", "NK-VK Dachrinne", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1202", "Zahlung Dachrinne", BuchungskontoTyp.Aktiv),
            };
            dachrinnenreinigung.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachNutzeinheit) { Umlage = dachrinnenreinigung });
            AddBkForderung(dachrinnenreinigung, 500, date, 2021);
            umlagen.Add(dachrinnenreinigung);

            var gartenpflege = new Umlage
            {
                Typ = new Umlagetyp("Gartenpflege"),
                NkVerrechnungsKonto = new Buchungskonto("7003", "NK-VK Garten", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1203", "Zahlung Garten", BuchungskontoTyp.Aktiv),
            };
            gartenpflege.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachNutzeinheit) { Umlage = gartenpflege });
            gartenpflege.Wohnungen.Add(wohnungen.First());
            AddBkForderung(gartenpflege, 650, date, 2021);
            umlagen.Add(gartenpflege);

            var allgemeinstrom = new Umlage
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung"),
                Wohnungen = wohnungen,
                NkVerrechnungsKonto = new Buchungskonto("7004", "NK-VK Strom", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1204", "Zahlung Strom", BuchungskontoTyp.Aktiv),
            };
            allgemeinstrom.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachWohnflaeche) { Umlage = allgemeinstrom });
            AddBkForderung(allgemeinstrom, 200, date, 2021);
            umlagen.Add(allgemeinstrom);

            var muellbeseitigung = new Umlage
            {
                Typ = new Umlagetyp("Müllbeseitigung"),
                Wohnungen = wohnungen,
                NkVerrechnungsKonto = new Buchungskonto("7005", "NK-VK Müll", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1205", "Zahlung Müll", BuchungskontoTyp.Aktiv),
            };
            muellbeseitigung.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachPersonenzahl) { Umlage = muellbeseitigung });
            AddBkForderung(muellbeseitigung, 1000, date, 2021);
            umlagen.Add(muellbeseitigung);

            var hkvo = new HKVO(new DateOnly(2000, 1, 1), 0.5m, 0.5m, HKVO_P9A2.Satz_2, 0.05m) { Betriebsstrom = allgemeinstrom };
            var heizkosten = new Umlage
            {
                Typ = new Umlagetyp("Heizkosten"),
                Wohnungen = wohnungen,
                NkVerrechnungsKonto = new Buchungskonto("7006", "NK-VK Heizung", BuchungskontoTyp.Passiv),
                ZahlungsKonto = new Buchungskonto("1206", "Zahlung Heizung", BuchungskontoTyp.Aktiv),
            };
            heizkosten.Versionen.Add(new UmlageVersion(new DateOnly(2000, 1, 1), Umlageschluessel.NachVerbrauch) { Umlage = heizkosten });
            hkvo.Heizkosten = heizkosten;
            heizkosten.HeizkostenHKVOs.Add(hkvo);
            AddBkForderung(heizkosten, 2000, date, 2021);

            var allgemeinZaehler = new Zaehler("Allgemein_Heizung", Zaehlertyp.Gas);
            var allgemeinZaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
            {
                Zaehler = allgemeinZaehler

            };
            var allgemeinZaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 25000)
            {
                Zaehler = allgemeinZaehler
            };
            allgemeinZaehler.Staende.Add(allgemeinZaehlerstand_beginn);
            allgemeinZaehler.Staende.Add(allgemeinZaehlerstand_ende);
            heizkosten.Zaehler.Add(allgemeinZaehler);
            foreach (var wohnung in wohnungen)
            {
                var zaehler = new Zaehler("Heizung", Zaehlertyp.Gas)
                {
                    Wohnung = wohnung
                };
                var zaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
                {
                    Zaehler = zaehler

                };
                var zaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 10000)
                {
                    Zaehler = zaehler
                };
                zaehler.Staende.Add(zaehlerstand_beginn);
                zaehler.Staende.Add(zaehlerstand_ende);
                heizkosten.Zaehler.Add(zaehler);
            }

            foreach (var wohnung in wohnungen)
            {
                var zaehler = new Zaehler("Heizung", Zaehlertyp.Warmwasser)
                {
                    Wohnung = wohnung
                };
                var zaehlerstand_beginn = new Zaehlerstand(new DateOnly(2021, 1, 1), 0)
                {
                    Zaehler = zaehler

                };
                var zaehlerstand_ende = new Zaehlerstand(new DateOnly(2021, 12, 31), 30)
                {
                    Zaehler = zaehler
                };
                zaehler.Staende.Add(zaehlerstand_beginn);
                zaehler.Staende.Add(zaehlerstand_ende);
                heizkosten.Zaehler.Add(zaehler);
            }
            umlagen.Add(heizkosten);

            return umlagen;
        }
    }
}
