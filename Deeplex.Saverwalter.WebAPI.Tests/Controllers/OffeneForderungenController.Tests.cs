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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietzahlungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.OffeneForderungenController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// Startseite und Forderungsstatus erkennen die Sollstellung eines Monats nach derselben
    /// Regel wie Buchen und Mietkonto (<see cref="Sollstellung"/>).
    /// </summary>
    public class OffeneForderungenControllerTests
    {
        private static Task ZahleAsync(SaverwalterContext ctx, Vertrag vertrag, DateOnly monat, DateOnly datum) =>
            new TransaktionBuchungsService(ctx).BucheAsync(new TransaktionBuchungsService.TransaktionsInput
            {
                Betrag = 650m,
                Zahlungsdatum = datum,
                Mieten =
                {
                    new TransaktionBuchungsService.MietzahlungsInput
                    {
                        VertragId = vertrag.VertragId,
                        BetreffenderMonat = monat,
                        Kaltmiete = 500m,
                        NkVorauszahlung = 150m,
                    }
                }
            });

        private static OffeneForderungenController AlsAdmin(SaverwalterContext ctx)
        {
            var admin = new UserAccount { Username = "admin", Name = "admin", Role = UserRole.Admin };
            ctx.UserAccounts.Add(admin);
            ctx.SaveChanges();
            return new OffeneForderungenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(admin.AssembleClaims(), "mock"))
                    }
                }
            };
        }

        private static async Task<Vertrag> VertragAbAsync(SaverwalterContext ctx, DateOnly beginn)
        {
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            await ctx.SaveChangesAsync();
            var version = vertrag.Versionen.Single();
            version.Beginn = beginn;
            version.Nebenkostenvorauszahlung = 150m;
            await ctx.SaveChangesAsync();
            return vertrag;
        }

        [Fact]
        public async Task MietKorrekturImLaufendenMonatVerdecktNichtDieOffeneMiete()
        {
            var ctx = TestUtils.GetContext();
            var heute = DateOnly.FromDateTime(DateTime.Today);
            var dieserMonat = new DateOnly(heute.Year, heute.Month, 1);
            var vormonat = dieserMonat.AddMonths(-1);
            var vertrag = await VertragAbAsync(ctx, vormonat);
            await ZahleAsync(ctx, vertrag, vormonat, vormonat.AddDays(4));
            vertrag.Versionen.Single().Grundmiete = 600m;
            await ctx.SaveChangesAsync();
            // Die Korrektur trägt das heutige Datum, liegt also im laufenden Monat.
            await new MietKorrekturService(ctx, new BuchungssatzSchutzService(ctx))
                .BucheDifferenzAsync(vertrag.VertragId, vormonat.Year, "Mieterhöhung nachgetragen");

            var result = await AlsAdmin(ctx).GetMietMatrix(heute.Year);

            var zeile = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<MietMatrixZeile>>().Subject.Single();
            zeile.Monate.Should().ContainSingle(m => m.Monat == dieserMonat.ToString("yyyy-MM-dd"))
                .Which.Should().Match<MietMatrixMonat>(m => m.Offen == 600m && m.Gezahlt == 0m);
        }

        [Fact]
        public async Task LesenLegtKeineSollstellungAnUndZeigtDenMonatTrotzdemAlsFaellig()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAbAsync(ctx, new DateOnly(2024, 1, 1));
            var garage = new GarageVertrag
            {
                Garage = new Garage("G1")
                {
                    Besitzer = vertrag.Ansprechpartner!,
                    Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
                },
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            garage.Versionen.Add(new GarageVertragVersion(new DateOnly(2024, 1, 1), 50m) { GarageVertrag = garage });
            ctx.GarageVertraege.Add(garage);
            await ctx.SaveChangesAsync();
            var vorher = ctx.Buchungssaetze.Count();
            var controller = AlsAdmin(ctx);

            var miete = (await controller.GetMietMatrix(2024)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<MietMatrixZeile>>().Subject.Single().Monate;
            var garagen = (await controller.GetGarageMatrix(2024)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<GarageMatrixZeile>>().Subject.Single().Monate;

            miete.Should().HaveCount(12);
            miete.Should().OnlyContain(m => m.Offen == 500m);
            garagen.Should().HaveCount(12);
            garagen.Should().OnlyContain(m => m.Offen == 50m);
            ctx.Buchungssaetze.Count().Should().Be(vorher);
        }

        [Fact]
        public async Task MietMatrixZeigtBezahlteUndOffeneMonateNebeneinander()
        {
            var ctx = TestUtils.GetContext();
            var januar = new DateOnly(2024, 1, 1);
            var vertrag = await VertragAbAsync(ctx, januar);
            await ZahleAsync(ctx, vertrag, januar, new DateOnly(2024, 1, 5));

            var result = await AlsAdmin(ctx).GetMietMatrix(2024);

            var zeile = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<MietMatrixZeile>>().Subject.Single();
            zeile.VertragId.Should().Be(vertrag.VertragId);
            zeile.Adresse.Should().NotBeNullOrWhiteSpace();
            zeile.Wohnung.Should().Be(vertrag.Wohnung.Bezeichnung);
            zeile.Mieter.Should().NotBeNullOrWhiteSpace();
            zeile.Monate.Should().HaveCount(12);
            zeile.Monate[0].Should().Match<MietMatrixMonat>(m => m.Soll == 500m && m.Gezahlt == 500m && m.Offen == 0m);
            zeile.Monate[1].Should().Match<MietMatrixMonat>(m => m.Soll == 500m && m.Gezahlt == 0m && m.Offen == 500m);
        }

        [Fact]
        public async Task GarageMatrixListetEigenstaendigeGaragenMitAllenMonaten()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = await VertragAbAsync(ctx, new DateOnly(2024, 1, 1));
            var garage = new GarageVertrag
            {
                Garage = new Garage("G1")
                {
                    Besitzer = vertrag.Ansprechpartner!,
                    Ertragskonto = new Buchungskonto("4100", "Garagenmiete", BuchungskontoTyp.Ertrag),
                },
                MietBuchungskonto = new Buchungskonto("1100", "Garage", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto("1101", "Garage Zahlung", BuchungskontoTyp.Aktiv),
            };
            garage.Versionen.Add(new GarageVertragVersion(new DateOnly(2024, 1, 1), 50m) { GarageVertrag = garage });
            ctx.GarageVertraege.Add(garage);
            await ctx.SaveChangesAsync();

            var result = await AlsAdmin(ctx).GetGarageMatrix(2024);

            var zeile = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<GarageMatrixZeile>>().Subject.Single();
            zeile.Garage.Should().Be("G1");
            zeile.Monate.Should().HaveCount(12);
            zeile.Monate.Should().OnlyContain(m => m.Soll == 50m && m.Gezahlt == 0m && m.Offen == 50m);
        }

        [Fact]
        public async Task ZaehlerMatrixListetAuchZaehlerMitStandImJahr()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            var zaehler = vertrag.Wohnung.Zaehler.First();
            zaehler.Staende.Add(new Zaehlerstand(new DateOnly(2024, 5, 1), 100m) { Zaehler = zaehler });
            await ctx.SaveChangesAsync();
            var controller = new OffeneForderungenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };

            var result = await controller.GetZaehlerMatrix(2024);

            var zeilen = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<ZaehlerMatrixZeile>>().Subject.ToList();
            zeilen.Should().ContainSingle(z => z.Id == zaehler.ZaehlerId)
                .Which.Should().Match<ZaehlerMatrixZeile>(z =>
                    z.Staende.Count == 1 && z.Staende[0].Stand == 100m
                    && z.LastStand == 100m && z.LastDatum == "2024-05-01");
            zeilen.Where(z => z.Id != zaehler.ZaehlerId).Should().OnlyContain(z => z.Staende.Count == 0);
        }

        [Fact]
        public async Task ForderungsstatusZaehltDieZahlungFuerDenZugeordnetenMonat()
        {
            var ctx = TestUtils.GetContext();
            var januar = new DateOnly(2024, 1, 1);
            var februar = new DateOnly(2024, 2, 1);
            var vertrag = await VertragAbAsync(ctx, januar);
            // Januarmiete, gezahlt im Februar.
            await ZahleAsync(ctx, vertrag, januar, new DateOnly(2024, 2, 3));

            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var controller = new MietzahlungController(ctx, auth);

            var jan = (await controller.GetForderungsstatus(vertrag.VertragId, januar)).Result
                .Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<ForderungsstatusEntry>().Subject;
            var feb = (await controller.GetForderungsstatus(vertrag.VertragId, februar)).Result
                .Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<ForderungsstatusEntry>().Subject;

            jan.SchonGezahlt.Should().Be(500m);
            jan.VerbleibendeForderung.Should().Be(0m);
            feb.SchonGezahlt.Should().Be(0m);
        }
    }
}
