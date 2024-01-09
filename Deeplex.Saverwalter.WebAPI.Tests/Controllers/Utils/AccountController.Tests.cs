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

using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.UserController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AccountControllerTests : IDisposable
    {
        public SaverwalterContext ctx;
        public AccountControllerTests()
        {
            ctx = TestUtils.GetContext();
        }

        public void Dispose()
        {
            ctx.Dispose();
        }

        [Fact]
        public async void SignInSuccess()
        {
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new UserController(ctx, tokenService, userService, A.Fake<HttpClient>());

            var account = new UserAccount
            {
                Username = "test1",
                Name = "Miss Test",
                Role = UserRole.User
            };

            var account2 = new UserAccount
            {
                Username = "test2",
                Name = "Miss Test",
                Role = UserRole.User
            };

            ctx.UserAccounts.Add(account);
            ctx.UserAccounts.Add(account2);
            ctx.SaveChanges();
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));
            await userService.UpdateUserPassword(account2, Encoding.UTF8.GetBytes("test2"));

            var result = await controller.SignIn(new SignInRequest
            {
                Username = "test1",
                Password = "test"
            });

            result.Value.Should().BeOfType<LoginResult>();
        }

        [Fact]
        public async void SignInFailed()
        {
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new UserController(ctx, tokenService, userService, A.Fake<HttpClient>());

            var account = new UserAccount
            {
                Username = "test3",
                Name = "Miss Test",
            };

            ctx.UserAccounts.Add(account);
            ctx.SaveChanges();
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));

            var result = await controller.SignIn(new SignInRequest
            {
                Username = "test3",
                Password = "test3"
            });

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
