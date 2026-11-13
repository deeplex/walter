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
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AbrechnungsresultatDbServiceTests
    {
        [Fact]
        public async Task PutVerlangtBearbeitungsrecht()
        {
            var ctx = TestUtils.GetContext();
            var vertrag = TestUtils.FillVertragWithSomeData(ctx, 500m);
            var resultat = new Abrechnungsresultat { Vertrag = vertrag };
            new AbrechnungsresultatBuchungsService(ctx).BucheAbrechnung(resultat, 2024, 100m);
            ctx.Abrechnungsresultate.Add(resultat);
            await ctx.SaveChangesAsync();

            // Nur Lese-Recht: Update wird verweigert, alles andere erlaubt.
            var user = A.Fake<ClaimsPrincipal>();
            var auth = A.Fake<IAuthorizationService>();
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._, A<IEnumerable<IAuthorizationRequirement>>._))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            A.CallTo(() => auth.AuthorizeAsync(user, A<object>._,
                    A<IEnumerable<IAuthorizationRequirement>>.That.Contains(Operations.Update)))
                .Returns(Task.FromResult(AuthorizationResult.Failed()));

            var service = new AbrechnungsresultatDbService(ctx, auth);
            var result = await service.Put(user, resultat.AbrechnungsresultatId,
                new AbrechnungsresultatEntry { Abgesendet = true });

            result.Result.Should().BeOfType<ForbidResult>();
            resultat.Abgesendet.Should().BeFalse();
        }
    }
}
