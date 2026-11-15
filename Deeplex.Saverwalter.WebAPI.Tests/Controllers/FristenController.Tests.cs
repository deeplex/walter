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
using static Deeplex.Saverwalter.WebAPI.Controllers.FristenController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FristenControllerTests
    {
        [Fact]
        public async Task MatrixListetJedesAbrechnungsjahrAuchOhneGesendeteAbrechnung()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            foreach (var version in vertrag.Versionen)
                version.Grundmiete = 500m;
            await ctx.SaveChangesAsync();
            var controller = new FristenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };

            var result = await controller.GetMatrix();

            var zeile = result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<AbrechnungsMatrixZeile>>().Subject
                .Single(z => z.VertragId == vertrag.VertragId);
            zeile.Wohnung.Should().Be(vertrag.Wohnung.Bezeichnung);
            zeile.Jahre.Should().NotBeEmpty();
            zeile.Jahre.Select(j => j.Jahr).Should().BeInAscendingOrder().And.OnlyHaveUniqueItems();
            zeile.Jahre.Should().OnlyContain(j => !j.Gesendet);
            zeile.Jahre.Should().OnlyContain(j => j.Fristablauf == new DateOnly(j.Jahr + 1, 12, 31));
        }

        [Fact]
        public async Task MatrixBlendetVertraegeOhneMieteUndNkVorauszahlungAus()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            foreach (var version in vertrag.Versionen)
            {
                version.Grundmiete = 0m;
                version.Nebenkostenvorauszahlung = 0m;
            }
            await ctx.SaveChangesAsync();
            var controller = new FristenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };

            var result = await controller.GetMatrix();

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<AbrechnungsMatrixZeile>>().Subject
                .Should().NotContain(z => z.VertragId == vertrag.VertragId);
        }

        [Fact]
        public async Task MatrixBlendetJahreMitAbrechnungsverzichtAus()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            foreach (var version in vertrag.Versionen)
                version.Grundmiete = 500m;
            await ctx.SaveChangesAsync();
            var controller = new FristenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };
            var vorher = (await controller.GetMatrix()).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<AbrechnungsMatrixZeile>>().Subject
                .Single(z => z.VertragId == vertrag.VertragId).Jahre;
            var verzichtsjahr = vorher[0].Jahr;
            ctx.Abrechnungsverzichte.Add(new Abrechnungsverzicht
            {
                Vertrag = vertrag,
                Jahr = verzichtsjahr,
                Grund = "Familienmitglied",
                Datum = new DateOnly(2026, 1, 1),
            });
            await ctx.SaveChangesAsync();

            var nachher = (await controller.GetMatrix()).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<AbrechnungsMatrixZeile>>().Subject
                .SingleOrDefault(z => z.VertragId == vertrag.VertragId);

            (nachher?.Jahre.Select(j => j.Jahr) ?? []).Should().NotContain(verzichtsjahr);
            (nachher?.Jahre.Count ?? 0).Should().Be(vorher.Count - 1);
        }

        [Fact]
        public async Task WohnungenMitAbrechnungOhneVerzichtSindNoetigMitVerzichtNicht()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            foreach (var version in vertrag.Versionen)
                version.Grundmiete = 500m;
            await ctx.SaveChangesAsync();
            var controller = new FristenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };
            var jahr = vertrag.Versionen.Min(v => v.Beginn).Year;

            var davor = (await controller.GetWohnungenMitAbrechnung(jahr)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<WohnungMitAbrechnung>>().Subject;
            ctx.Abrechnungsverzichte.Add(new Abrechnungsverzicht
            {
                Vertrag = vertrag,
                Jahr = jahr,
                Grund = "Familienmitglied",
                Datum = new DateOnly(2026, 1, 1),
            });
            await ctx.SaveChangesAsync();
            var danach = (await controller.GetWohnungenMitAbrechnung(jahr)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<WohnungMitAbrechnung>>().Subject;

            davor.Should().ContainSingle(w => w.WohnungId == vertrag.Wohnung.WohnungId).Which.Abgeschlossen.Should().BeFalse();
            danach.Should().NotContain(w => w.WohnungId == vertrag.Wohnung.WohnungId);
        }

        [Fact]
        public async Task WohnungIstAbgeschlossenWennDieAbrechnungDesJahresGesendetIst()
        {
            var ctx = TestUtils.GetContext();
            var user = new UserAccount { Username = "test", Name = "test", Role = UserRole.User };
            ctx.UserAccounts.Add(user);
            var vertrag = TestUtils.GetVertragForAbrechnung(ctx);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Eigentuemer) { UserAccount = user, Wohnung = vertrag.Wohnung });
            foreach (var version in vertrag.Versionen)
                version.Grundmiete = 500m;
            var jahr = vertrag.Versionen.Min(v => v.Beginn).Year;
            var satz = new Buchungssatz(new DateOnly(jahr + 1, 3, 1), $"BK-Abrechnung {jahr}") { Buchungsjahr = jahr };
            ctx.Buchungssaetze.Add(satz);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat { Vertrag = vertrag, Buchungssatz = satz, Abgesendet = false });
            await ctx.SaveChangesAsync();
            var controller = new FristenController(ctx)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(user.AssembleClaims(), "mock"))
                    }
                }
            };

            var entwurf = (await controller.GetWohnungenMitAbrechnung(jahr)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<WohnungMitAbrechnung>>().Subject.Single();
            ctx.Abrechnungsresultate.Single().Abgesendet = true;
            await ctx.SaveChangesAsync();
            var gesendet = (await controller.GetWohnungenMitAbrechnung(jahr)).Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<WohnungMitAbrechnung>>().Subject.Single();

            entwurf.Abgeschlossen.Should().BeFalse();
            gesendet.Abgeschlossen.Should().BeTrue();
        }
    }
}
