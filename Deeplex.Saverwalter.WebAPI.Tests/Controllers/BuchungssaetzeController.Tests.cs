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
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungssaetzeController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class BuchungssaetzeControllerTests
    {
        /// <summary>Die Wohnungs-Autorisierung selbst ist in den PermissionHandler-Tests abgedeckt.</summary>
        private class AllowAllAuth : IAuthorizationService
        {
            public Task<AuthorizationResult> AuthorizeAsync(
                ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
                Task.FromResult(AuthorizationResult.Success());

            public Task<AuthorizationResult> AuthorizeAsync(
                ClaimsPrincipal user, object? resource, string policyName) =>
                Task.FromResult(AuthorizationResult.Success());
        }

        private static BuchungssaetzeController WithUser(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            var controller = new BuchungssaetzeController(
                new StornoBuchungsService(ctx), new BuchungssatzSchutzService(ctx), ctx, new AllowAllAuth())
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

        private static Buchungssatz AddSatz(SaverwalterContext ctx, Vertrag vertrag, string beschreibung, decimal betrag)
        {
            var satz = new Buchungssatz(new DateOnly(2026, 5, 1), beschreibung);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.MietBuchungskonto
            });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            ctx.Buchungssaetze.Add(satz);
            return satz;
        }

        /// <summary>
        /// Zwei Wohnungen mit Verträgen und je einem Buchungssatz. Der Verwalter
        /// verwaltet nur die erste Wohnung.
        /// </summary>
        private static (UserAccount manager, Buchungssatz managedSatz, Buchungssatz foreignSatz, Vertrag managedVertrag) Seed(SaverwalterContext ctx)
        {
            var manager = new UserAccount { Username = "m", Name = "m", Role = UserRole.User };
            ctx.UserAccounts.Add(manager);

            var v1 = TestUtils.FillVertragWithSomeData(ctx, 0);
            var v2 = TestUtils.FillVertragWithSomeData(ctx, 0);
            ctx.VerwalterSet.Add(new Verwalter(VerwalterRolle.Vollmacht) { UserAccount = manager, Wohnung = v1.Wohnung });

            var managedSatz = AddSatz(ctx, v1, "Mietzahlung Mai", 500m);
            var foreignSatz = AddSatz(ctx, v2, "Fremde Buchung", 700m);
            ctx.SaveChanges();

            return (manager, managedSatz, foreignSatz, v1);
        }

        [Fact]
        public async Task GetList_returns_only_saetze_on_managed_konten()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.GetList(new PagedQuery(), kontoId: null)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(1);
            paged.Items.Single().Id.Should().Be(managedSatz.BuchungssatzId);
        }

        [Fact]
        public async Task GetList_returns_all_saetze_for_admin()
        {
            var ctx = TestUtils.GetContext();
            Seed(ctx);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var result = (await controller.GetList(new PagedQuery(), kontoId: null)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetList_filters_by_konto()
        {
            var ctx = TestUtils.GetContext();
            var (_, managedSatz, _, vertrag) = Seed(ctx);
            var admin = new UserAccount { Username = "a", Name = "a", Role = UserRole.Admin };
            var controller = WithUser(ctx, Principal(admin));

            var result = (await controller.GetList(
                new PagedQuery(), vertrag.MietBuchungskonto.BuchungskontoId)).Result as OkObjectResult;
            var paged = result!.Value as PagedResult<BuchungssatzEntryBase>;

            paged!.TotalCount.Should().Be(1);
            var entry = paged.Items.Single();
            entry.Id.Should().Be(managedSatz.BuchungssatzId);
            // Kontoblatt: Soll/Haben aus Sicht des gefilterten Kontos
            entry.KontoSoll.Should().Be(500m);
            entry.KontoHaben.Should().BeNull();
        }

        [Fact]
        public async Task Get_managed_satz_includes_zeilen_und_verknuepfungen()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, vertrag) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.Get(managedSatz.BuchungssatzId)).Result as OkObjectResult;
            var entry = result!.Value as BuchungssatzEntry;

            entry!.Zeilen.Should().HaveCount(2);
            entry.Betrag.Should().Be(500m);
            entry.Verknuepfungen.Should().Contain(v =>
                v.Typ == "Vertrag" && v.Id == vertrag.VertragId.ToString());
        }

        [Fact]
        public async Task Get_und_kontoblatt_zeigen_opos_ausgleich()
        {
            var ctx = TestUtils.GetContext();
            var (manager, forderung, _, vertrag) = Seed(ctx);

            // Zahlung über 300 € gleicht die Forderung (Soll 500 auf dem Mietkonto)
            // teilweise aus: Forderungszeile (Soll) <-> Zahlungszeile (Haben).
            var zahlung = new Buchungssatz(new DateOnly(2026, 5, 10), "Teilzahlung Mai");
            zahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 300m)
            {
                Buchungssatz = zahlung,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            var habenZeile = new Buchungszeile(SollHaben.Haben, 300m)
            {
                Buchungssatz = zahlung,
                Buchungskonto = vertrag.MietBuchungskonto
            };
            zahlung.Buchungszeilen.Add(habenZeile);
            ctx.Buchungssaetze.Add(zahlung);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = forderung.Buchungszeilen.Single(z =>
                    z.SollHaben == SollHaben.Soll),
                HabenZeile = habenZeile
            });
            ctx.SaveChanges();

            var controller = WithUser(ctx, Principal(manager));

            // Detail: Forderungszeile ist teilweise ausgeglichen und verlinkt die Zahlung
            var detailResult = (await controller.Get(forderung.BuchungssatzId)).Result as OkObjectResult;
            var detail = detailResult!.Value as BuchungssatzEntry;
            var sollZeile = detail!.Zeilen.Single(z =>
                z.SollHaben == "Soll" && z.KontoId == vertrag.MietBuchungskonto.BuchungskontoId);
            sollZeile.Ausgleichbar.Should().BeTrue();
            sollZeile.Ausgeglichen.Should().Be(300m);
            sollZeile.Offen.Should().Be(200m);
            sollZeile.Ausgleiche.Should().ContainSingle(a =>
                a.BuchungssatzId == zahlung.BuchungssatzId && a.Betrag == 300m);

            // Kontoblatt des Mietkontos: Forderung 200 € offen, Zahlung voll zugeordnet
            var listResult = (await controller.GetList(
                new PagedQuery(), vertrag.MietBuchungskonto.BuchungskontoId)).Result as OkObjectResult;
            var rows = (listResult!.Value as PagedResult<BuchungssatzEntryBase>)!.Items.ToList();
            rows.Single(r => r.Id == forderung.BuchungssatzId).KontoOffen.Should().Be(200m);
            rows.Single(r => r.Id == zahlung.BuchungssatzId).KontoOffen.Should().Be(0m);
        }

        [Fact]
        public async Task Delete_freier_satz_wird_geloescht()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Delete(managedSatz.BuchungssatzId);

            result.Should().BeOfType<OkResult>();
            ctx.Buchungssaetze.Any(s => s.BuchungssatzId == managedSatz.BuchungssatzId)
                .Should().BeFalse();
        }

        [Fact]
        public async Task Delete_mit_opos_verknuepfung_ist_conflict()
        {
            var ctx = TestUtils.GetContext();
            var (manager, forderung, _, vertrag) = Seed(ctx);
            var zahlung = AddSatz(ctx, vertrag, "Zahlung", 500m);
            ctx.OffenePostenAusgleiche.Add(new OffenerPostenAusgleich
            {
                SollZeile = forderung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Soll),
                HabenZeile = zahlung.Buchungszeilen.Single(z => z.SollHaben == SollHaben.Haben)
            });
            ctx.SaveChanges();
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Delete(forderung.BuchungssatzId);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Delete_und_storno_sind_nach_abrechnung_gesperrt()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, _, vertrag) = Seed(ctx);

            // NK-Vorauszahlung des Jahres 2026
            var vorauszahlung = new Buchungssatz(new DateOnly(2026, 4, 1), "NK-Vorauszahlung April");
            vorauszahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m)
            {
                Buchungssatz = vorauszahlung,
                Buchungskonto = vertrag.ZahlungsKonto
            });
            vorauszahlung.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, 100m)
            {
                Buchungssatz = vorauszahlung,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            ctx.Buchungssaetze.Add(vorauszahlung);

            // Abrechnung 2026 existiert
            var abrechnungssatz = new Buchungssatz(new DateOnly(2026, 12, 31), "BK-Abrechnung 2026");
            abrechnungssatz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, 100m)
            {
                Buchungssatz = abrechnungssatz,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            ctx.Buchungssaetze.Add(abrechnungssatz);
            ctx.Abrechnungsresultate.Add(new Abrechnungsresultat
            {
                Vertrag = vertrag,
                Buchungssatz = abrechnungssatz
            });
            ctx.SaveChanges();

            var controller = WithUser(ctx, Principal(manager));

            // Eingeflossene Vorauszahlung: weder löschen noch stornieren
            (await controller.Delete(vorauszahlung.BuchungssatzId))
                .Should().BeOfType<ConflictObjectResult>();
            (await controller.Stornieren(vorauszahlung.BuchungssatzId, new StornoRequest { Grund = "Test" }))
                .Result.Should().BeOfType<ConflictObjectResult>();

            // Abrechnungssatz selbst: nur über den Abrechnungslauf rückabwickelbar
            (await controller.Delete(abrechnungssatz.BuchungssatzId))
                .Should().BeOfType<ConflictObjectResult>();
            (await controller.Stornieren(abrechnungssatz.BuchungssatzId, new StornoRequest { Grund = "Test" }))
                .Result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Storno_ohne_grund_ist_bad_request()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Stornieren(
                managedSatz.BuchungssatzId, new StornoRequest { Grund = "  " });

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Storno_schreibt_grund_in_notiz()
        {
            var ctx = TestUtils.GetContext();
            var (manager, managedSatz, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = (await controller.Stornieren(
                managedSatz.BuchungssatzId,
                new StornoRequest { Grund = "Falscher Betrag erfasst" })).Result as OkObjectResult;

            var info = result!.Value as StornoBuchungssatzInfo;
            var storno = ctx.Buchungssaetze.Single(s => s.BuchungssatzId == info!.BuchungssatzId);
            storno.Notiz.Should().Be("Falscher Betrag erfasst");
            storno.StornoVon!.BuchungssatzId.Should().Be(managedSatz.BuchungssatzId);
        }

        [Fact]
        public async Task Get_foreign_satz_is_forbidden()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, foreignSatz, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(foreignSatz.BuchungssatzId);

            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Get_missing_satz_is_not_found()
        {
            var ctx = TestUtils.GetContext();
            var (manager, _, _, _) = Seed(ctx);
            var controller = WithUser(ctx, Principal(manager));

            var result = await controller.Get(Guid.NewGuid());

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
