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

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Services.JahresabschlussService;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class JahresabschlussControllerTests
    {
        private static JahresabschlussController WithUser(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            var controller = new JahresabschlussController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            return controller;
        }

        private static ClaimsPrincipal Principal(UserAccount user) =>
            new(new ClaimsIdentity(user.AssembleClaims(), "mock"));

        /// <summary>
        /// Zwei Wohnungen mit je 7 Konten (2 Wohnung + 5 Vertrag). Der Verwalter
        /// verwaltet nur die erste.
        /// </summary>
        private static (UserAccount manager, Vertrag managed, Vertrag foreign) Seed(SaverwalterContext ctx)
        {
            var manager = new UserAccount { Username = "m", Name = "m", Role = UserRole.User };
            ctx.UserAccounts.Add(manager);

            var v1 = TestUtils.FillVertragWithSomeData(ctx, 0);
            var v2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Vollmacht) { UserAccount = manager, Wohnung = v1.Wohnung });
            ctx.SaveChanges();

            return (manager, v1, v2);
        }

        /// <summary>Bucht eine Mietforderung: Soll MietBuchungskonto / Haben MietErtragskonto.</summary>
        private static Buchungszeile BucheForderung(SaverwalterContext ctx, Vertrag vertrag, int jahr, decimal betrag)
        {
            var satz = new Buchungssatz(new DateOnly(jahr, 5, 3), $"Mietsoll {jahr}");
            var sollZeile = new Buchungszeile(SollHaben.Soll, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.MietBuchungskonto
            };
            satz.Buchungszeilen.Add(sollZeile);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.Wohnung.MietErtragskonto
            });
            ctx.Buchungssaetze.Add(satz);
            ctx.SaveChanges();
            return sollZeile;
        }

        /// <summary>
        /// Bucht eine Zahlung (Soll ZahlungsKonto / Haben MietBuchungskonto) und legt
        /// optional den OPOS-Ausgleich zur gegebenen Soll-Zeile an.
        /// </summary>
        private static Buchungszeile BucheZahlung(
            SaverwalterContext ctx, Vertrag vertrag, int jahr, decimal betrag, Buchungszeile? sollZeile)
        {
            var satz = new Buchungssatz(new DateOnly(jahr, 5, 20), $"Zahlung {jahr}");
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            var habenZeile = new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.MietBuchungskonto
            };
            satz.Buchungszeilen.Add(habenZeile);
            ctx.Buchungssaetze.Add(satz);
            if (sollZeile != null)
            {
                ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
                {
                    SollZeile = sollZeile,
                    HabenZeile = habenZeile
                });
            }
            ctx.SaveChanges();
            return habenZeile;
        }

        private static void ErstelleResultat(SaverwalterContext ctx, Vertrag vertrag, int jahr, bool abgesendet)
        {
            var satz = new Buchungssatz(new DateOnly(jahr + 1, 3, 1), $"BK-Abrechnung {jahr}")
            {
                Buchungsjahr = jahr
            };
            ctx.Buchungssaetze.Add(satz);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat
            {
                Vertrag = vertrag,
                Buchungssatz = satz,
                Abgesendet = abgesendet
            });
            ctx.SaveChanges();
        }

        private static async Task<JahresabschlussEntry> GetJahr(JahresabschlussController controller, int jahr)
        {
            var result = (await controller.Get(jahr)).Result as OkObjectResult;
            return (result!.Value as JahresabschlussEntry)!;
        }

        [Fact]
        public async Task Ausgeglichenes_konto_und_abgesendete_abrechnung_schliessen_das_jahr_ab()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 500m, sollZeile);
            ErstelleResultat(ctx, managed, 2024, abgesendet: true);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var mietKonto = abschluss.Konten.Single(k => k.Funktion == "Mietforderungen");
            mietKonto.SollJahr.Should().Be(500m);
            mietKonto.HabenJahr.Should().Be(500m);
            mietKonto.Endsaldo.Should().Be(0m);
            mietKonto.OffenePostenAnzahl.Should().Be(0);
            mietKonto.Ausgeglichen.Should().BeTrue();
            abschluss.KontenOffen.Should().Be(0);
            abschluss.AbrechnungenGesamt.Should().Be(1);
            abschluss.AbrechnungenFertig.Should().Be(1);
            abschluss.JahrAbgeschlossen.Should().BeTrue();
        }

        [Fact]
        public async Task Offene_forderung_traegt_sich_ins_folgejahr_vor()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss2024 = await GetJahr(controller, 2024);
            var abschluss2025 = await GetJahr(controller, 2025);

            var konto2024 = abschluss2024.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto2024.Ausgeglichen.Should().BeFalse();
            konto2024.Endsaldo.Should().Be(500m);
            konto2024.OffenePostenAnzahl.Should().Be(1);
            konto2024.OffenePostenBetrag.Should().Be(500m);
            abschluss2024.KontenOffen.Should().Be(1);
            abschluss2024.JahrAbgeschlossen.Should().BeFalse();

            var konto2025 = abschluss2025.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto2025.Saldovortrag.Should().Be(500m);
            konto2025.SollJahr.Should().Be(0m);
            konto2025.Endsaldo.Should().Be(500m);
            konto2025.Ausgeglichen.Should().BeFalse();
        }

        [Fact]
        public async Task Ausgleich_im_folgejahr_laesst_das_alte_jahr_offen()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2025, 500m, sollZeile);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss2024 = await GetJahr(controller, 2024);
            var abschluss2025 = await GetJahr(controller, 2025);

            var konto2024 = abschluss2024.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto2024.OffenePostenAnzahl.Should().Be(1);
            konto2024.Ausgeglichen.Should().BeFalse();

            var konto2025 = abschluss2025.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto2025.OffenePostenAnzahl.Should().Be(0);
            konto2025.Endsaldo.Should().Be(0m);
            konto2025.Ausgeglichen.Should().BeTrue();
        }

        [Fact]
        public async Task Teilausgleich_zeigt_verbleibenden_offenen_betrag()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 300m, sollZeile);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var konto = abschluss.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto.OffenePostenAnzahl.Should().Be(1);
            konto.OffenePostenBetrag.Should().Be(200m);
            konto.Endsaldo.Should().Be(200m);
            konto.Ausgeglichen.Should().BeFalse();
        }

        [Fact]
        public async Task Saldo_null_ohne_opos_ausgleich_bleibt_offen()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 500m, sollZeile: null);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var konto = abschluss.Konten.Single(k => k.Funktion == "Mietforderungen");
            konto.Endsaldo.Should().Be(0m);
            konto.OffenePostenAnzahl.Should().Be(1);
            konto.Ausgeglichen.Should().BeFalse();
        }

        [Fact]
        public async Task Abrechnungsstatus_unterscheidet_fehlend_erstellt_und_abgesendet()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 500m, sollZeile);
            var controller = WithUser(ctx, Principal(manager));

            var fehlt = (await GetJahr(controller, 2024)).Abrechnungen.Single();
            fehlt.ResultatVorhanden.Should().BeFalse();
            fehlt.Abgesendet.Should().BeFalse();

            ErstelleResultat(ctx, managed, 2024, abgesendet: false);
            var erstellt = await GetJahr(controller, 2024);
            erstellt.Abrechnungen.Single().ResultatVorhanden.Should().BeTrue();
            erstellt.Abrechnungen.Single().Abgesendet.Should().BeFalse();
            erstellt.AbrechnungenFertig.Should().Be(0);
            erstellt.JahrAbgeschlossen.Should().BeFalse();
        }

        private static void ErstelleVerzicht(SaverwalterContext ctx, Vertrag vertrag, int jahr, string grund)
        {
            ctx.Abrechnungsverzichte.Add(new Abrechnungsverzicht
            {
                Vertrag = vertrag,
                Jahr = jahr,
                Grund = grund,
                Datum = new DateOnly(jahr + 1, 1, 15)
            });
            ctx.SaveChanges();
        }

        [Fact]
        public async Task Abrechnungsverzicht_gilt_als_erledigt_und_schliesst_das_jahr_ab()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 500m, sollZeile);
            ErstelleVerzicht(ctx, managed, 2024, "Zeitraum vor Programmeinführung");
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var status = abschluss.Abrechnungen.Single();
            status.Verzichtet.Should().BeTrue();
            status.VerzichtGrund.Should().Be("Zeitraum vor Programmeinführung");
            status.Abgesendet.Should().BeFalse();
            status.ResultatVorhanden.Should().BeFalse();
            abschluss.AbrechnungenFertig.Should().Be(1);
            abschluss.JahrAbgeschlossen.Should().BeTrue();
        }

        [Fact]
        public async Task Verzicht_wirkt_nur_im_betroffenen_jahr()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile = BucheForderung(ctx, managed, 2024, 500m);
            BucheZahlung(ctx, managed, 2024, 500m, sollZeile);
            ErstelleVerzicht(ctx, managed, 2023, "anderes Jahr");
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var status = abschluss.Abrechnungen.Single();
            status.Verzichtet.Should().BeFalse();
            abschluss.AbrechnungenFertig.Should().Be(0);
            abschluss.JahrAbgeschlossen.Should().BeFalse();
        }

        [Fact]
        public async Task Verzicht_markiert_auch_die_offenen_Mietforderungen_als_verzichtet()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m); // offene Mietforderung, keine Zahlung
            ErstelleVerzicht(ctx, managed, 2024, "vor Programmeinführung");
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var mietKonto = abschluss.Konten.Single(k => k.Funktion == "Mietforderungen");
            mietKonto.OffenePostenAnzahl.Should().Be(1); // Forderung ist real offen …
            mietKonto.Verzichtet.Should().BeTrue();       // … zählt aber als verzichtet
            mietKonto.Ausgeglichen.Should().BeTrue();
            abschluss.KontenOffen.Should().Be(0);
            abschluss.JahrAbgeschlossen.Should().BeTrue();
        }

        [Fact]
        public async Task Ohne_Verzicht_bleibt_offene_Mietforderung_ein_offener_Punkt()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            var mietKonto = abschluss.Konten.Single(k => k.Funktion == "Mietforderungen");
            mietKonto.Verzichtet.Should().BeFalse();
            mietKonto.Ausgeglichen.Should().BeFalse();
            abschluss.KontenOffen.Should().Be(1);
        }

        [Fact]
        public async Task Verwalter_sieht_nur_verwaltete_konten_und_vertraege()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, foreign) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m);
            BucheForderung(ctx, foreign, 2024, 700m);
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            abschluss.Konten.Should().OnlyContain(k =>
                k.KontoId == managed.MietBuchungskonto.BuchungskontoId ||
                k.KontoId == managed.Wohnung.MietErtragskonto.BuchungskontoId);
            abschluss.Abrechnungen.Should().ContainSingle(a => a.VertragId == managed.VertragId);
        }

        [Fact]
        public async Task Admin_sieht_alle_konten_und_vertraege()
        {
            var ctx = TestUtils.GetContext();
            var (_, managed, foreign) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m);
            BucheForderung(ctx, foreign, 2024, 700m);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var abschluss = await GetJahr(controller, 2024);

            abschluss.Konten.Should().HaveCount(4);
            abschluss.Abrechnungen.Should().HaveCount(2);
        }

        [Fact]
        public async Task Uebersicht_liefert_jahre_absteigend_mit_status()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            var sollZeile2023 = BucheForderung(ctx, managed, 2023, 500m);
            BucheZahlung(ctx, managed, 2023, 500m, sollZeile2023);
            ErstelleResultat(ctx, managed, 2023, abgesendet: true);
            BucheForderung(ctx, managed, 2024, 500m);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.GetUebersicht()).Result as OkObjectResult;
            var uebersicht = (result!.Value as IEnumerable<JahresUebersichtEntry>)!.ToList();

            uebersicht.Select(u => u.Jahr).Should().ContainInOrder(2024, 2023);
            var jahr2023 = uebersicht.Single(u => u.Jahr == 2023);
            jahr2023.KontenOffen.Should().Be(0);
            jahr2023.AbrechnungenOffen.Should().Be(0);
            jahr2023.Abgeschlossen.Should().BeTrue();
            var jahr2024 = uebersicht.Single(u => u.Jahr == 2024);
            jahr2024.KontenOffen.Should().Be(1);
            jahr2024.AbrechnungenOffen.Should().Be(1);
            jahr2024.Abgeschlossen.Should().BeFalse();
        }

        [Fact]
        public async Task Stornierte_abrechnung_zaehlt_nicht_als_vorhanden()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managed, _) = Seed(ctx);
            BucheForderung(ctx, managed, 2024, 500m);
            var satz = new Buchungssatz(new DateOnly(2025, 3, 1), "BK-Abrechnung 2024") { Buchungsjahr = 2024 };
            var storno = new Buchungssatz(new DateOnly(2025, 4, 1), "Storno") { Buchungsjahr = 2024, StornoVon = satz };
            ctx.Buchungssaetze.AddRange(satz, storno);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat
            {
                Vertrag = managed,
                Buchungssatz = satz,
                Abgesendet = true
            });
            ctx.SaveChanges();
            var controller = WithUser(ctx, Principal(manager));

            var abschluss = await GetJahr(controller, 2024);

            abschluss.Abrechnungen.Single().ResultatVorhanden.Should().BeFalse();
        }
    }
}
