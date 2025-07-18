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
            optionsBuilder.UseInMemoryDatabase("TestDb");
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            var ctx = new SaverwalterContext(optionsBuilder.Options);

            return ctx;
        }
        public static Vertrag FillVertragWithSomeData(SaverwalterContext ctx, double grundmiete)
        {
            var vermieter = new Kontakt("TestKopf", Rechtsform.natuerlich)
            {
                Anrede = Anrede.Frau,
                Vorname = "TestVorname",
                Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt")
            };

            ctx.Kontakte.Add(vermieter);
            ctx.SaveChanges();

            var wohnung = new Wohnung("TestWohnung", 100, 100, 100, 1)
            {
                Adresse = new Adresse("TestStraße", "TestHausnummer", "TestPLZ", "TestOrt"),
                Besitzer = vermieter
            };

            var vertrag = new Vertrag()
            {
                Ansprechpartner = vermieter,
                Wohnung = wohnung
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

        public static List<Miete> Add12Mieten(Vertrag vertrag, double grundmiete)
        {
            var mieten = new List<Miete>();
            for (int month = 1; month <= 12; month++)
            {
                mieten.Add(new Miete(
                    new DateOnly(2021, month, 1),
                    new DateOnly(2021, month, 1),
                    grundmiete)
                {
                    Vertrag = vertrag,
                });
            }

            return mieten;
        }

        public static List<Miete> Add6Mieten(Vertrag vertrag, double grundmiete)
        {
            var mieten = new List<Miete>();
            for (int month = 1; month <= 6; month++)
            {
                mieten.Add(new Miete(
                    new DateOnly(2021, month, 1),
                    new DateOnly(2021, month, 1),
                    grundmiete)
                {
                    Vertrag = vertrag,
                });
            }

            return mieten;
        }

        public static List<Umlage> Add6Umlagen(List<Wohnung> wohnungen)
        {
            var umlagen = new List<Umlage>();
            var date = new DateOnly(2021, 1, 1);

            var grundsteuer = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Grundsteuer"),
                Wohnungen = wohnungen
            };
            var grundsteuer_rechnung = new Betriebskostenrechnung(1000, date, 2021) { Umlage = grundsteuer };
            grundsteuer.Betriebskostenrechnungen.Add(grundsteuer_rechnung);
            umlagen.Add(grundsteuer);

            var dachrinnenreinigung = new Umlage(Umlageschluessel.NachNutzeinheit)
            {
                Typ = new Umlagetyp("Dachrinnenreinigung"),
                Wohnungen = wohnungen
            };
            var dachrinnenreinigung_rechnung = new Betriebskostenrechnung(500, date, 2021) { Umlage = dachrinnenreinigung };
            dachrinnenreinigung.Betriebskostenrechnungen.Add(dachrinnenreinigung_rechnung);
            umlagen.Add(dachrinnenreinigung);

            var gartenpflege = new Umlage(Umlageschluessel.NachNutzeinheit)
            {
                Typ = new Umlagetyp("Gartenpflege")
            };
            gartenpflege.Wohnungen.Add(wohnungen.First());
            var gartenpflege_rechnung = new Betriebskostenrechnung(650, date, 2021) { Umlage = gartenpflege };
            gartenpflege.Betriebskostenrechnungen.Add(gartenpflege_rechnung);
            umlagen.Add(gartenpflege);

            var allgemeinstrom = new Umlage(Umlageschluessel.NachWohnflaeche)
            {
                Typ = new Umlagetyp("Allgemeinstrom/Hausbeleuchtung"),
                Wohnungen = wohnungen
            };
            var allgemeinstrom_rechnung = new Betriebskostenrechnung(200, date, 2021) { Umlage = allgemeinstrom };
            allgemeinstrom.Betriebskostenrechnungen.Add(allgemeinstrom_rechnung);
            umlagen.Add(allgemeinstrom);

            var muellbeseitigung = new Umlage(Umlageschluessel.NachPersonenzahl)
            {
                Typ = new Umlagetyp("Müllbeseitigung"),
                Wohnungen = wohnungen
            };
            var muellbeseitigung_rechnung = new Betriebskostenrechnung(1000, date, 2021) { Umlage = muellbeseitigung };
            muellbeseitigung.Betriebskostenrechnungen.Add(muellbeseitigung_rechnung);
            umlagen.Add(muellbeseitigung);

            var hkvo = new HKVO(0.5, 0.5, HKVO_P9A2.Satz_2, 0.05) { Betriebsstrom = allgemeinstrom };
            var heizkosten = new Umlage(Umlageschluessel.NachVerbrauch)
            {
                Typ = new Umlagetyp("Heizkosten"),
                Wohnungen = wohnungen,
                HKVO = hkvo
            };
            var heizkosten_rechnung = new Betriebskostenrechnung(2000, date, 2021) { Umlage = heizkosten };
            heizkosten.Betriebskostenrechnungen.Add(heizkosten_rechnung);

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
