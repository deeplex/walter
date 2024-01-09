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
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Services;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async void CreateUserAccount()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);

            var result = await userService.CreateUserAccount("test", "Miss test");

            result.Should().BeOfType<UserAccount>();
            result.Username.Should().Be("test");
        }

        [Fact]
        public async void SignInAsync()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var account = await userService.CreateUserAccount("sign_in_async_user", "Mister sign_in_async_user");
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));

            var result = await userService.SignInAsync("sign_in_async_user", "test");

            result.Succeeded.Should().Be(true);
            result.Account.Should().Be(account);
            result.SessionToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void SignInAsyncFailed()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var account = await userService.CreateUserAccount("test", "Miss test");
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));

            var result = await userService.SignInAsync("test", "test2");

            result.Succeeded.Should().Be(false);
        }
    }
}
