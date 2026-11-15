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

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class FileFamilyEndpointTests
    {
        private static readonly DateTimeOffset Juni2023 = new(2023, 6, 15, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task ZaehlerstandZeigtDieFamilieDesZaehlersUndDasJahrSeinesDatums()
        {
            var s3 = new FakeS3();
            Environment.SetEnvironmentVariable("S3_PROVIDER", FakeS3.Base);

            var ctx = TestUtils.GetContext();
            var zaehler = new Zaehler("Z1", Zaehlertyp.Strom);
            var stand = new Zaehlerstand(new DateOnly(2023, 12, 31), 4000m) { Zaehler = zaehler };
            var anderer = new Zaehlerstand(new DateOnly(2022, 12, 31), 3000m) { Zaehler = zaehler };
            zaehler.Staende.Add(stand);
            zaehler.Staende.Add(anderer);
            ctx.ZaehlerSet.Add(zaehler);
            await ctx.SaveChangesAsync();
            s3.Add($"zaehler/{zaehler.ZaehlerId}/2023/typenschild.jpg", "x", Juni2023);
            s3.Add($"zaehlerstaende/{stand.ZaehlerstandId}/2023/stand.jpg", "x", Juni2023);
            s3.Add($"zaehlerstaende/{anderer.ZaehlerstandId}/2022/alt.jpg", "x", Juni2023);

            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var controller = new ZaehlerstandController(
                A.Fake<ILogger<ZaehlerstandController>>(), new ZaehlerstandDbService(ctx, auth), new HttpClient(s3));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Path = $"/api/zaehlerstaende/{stand.ZaehlerstandId}/files/family";

            var result = await controller.GetFileFamily(stand.ZaehlerstandId);

            var family = result.Result.Should().BeOfType<OkObjectResult>().Which.Value
                .Should().BeOfType<FileControllerBase<ZaehlerstandController.ZaehlerstandEntry, int, Zaehlerstand>.FileFamilyResult>().Subject;
            family.DefaultYear.Should().Be(2023);
            family.Files.Select(f => (f.FileName, f.Source)).Should().BeEquivalentTo(new[]
            {
                ("typenschild.jpg", (string?)"Zähler"),
                ("stand.jpg", null),
                ("alt.jpg", "Zählerstand 31.12.2022"),
            });
        }

        [Fact]
        public async Task ObjektOhneEigenesJahrHatKeinVoreingestelltesJahr()
        {
            var s3 = new FakeS3();
            Environment.SetEnvironmentVariable("S3_PROVIDER", FakeS3.Base);
            var ctx = TestUtils.GetContext();
            var zaehler = new Zaehler("Z1", Zaehlertyp.Strom);
            ctx.ZaehlerSet.Add(zaehler);
            await ctx.SaveChangesAsync();

            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(A<ClaimsPrincipal>._, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            var controller = new ZaehlerController(
                A.Fake<ILogger<ZaehlerController>>(), new ZaehlerDbService(ctx, auth), new HttpClient(s3));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Path = $"/api/zaehler/{zaehler.ZaehlerId}/files/family";

            var result = await controller.GetFileFamily(zaehler.ZaehlerId);

            result.Result.Should().BeOfType<OkObjectResult>().Which.Value
                .Should().BeOfType<FileControllerBase<ZaehlerController.ZaehlerEntry, int, Zaehler>.FileFamilyResult>()
                .Which.DefaultYear.Should().Be(0);
        }
    }
}
