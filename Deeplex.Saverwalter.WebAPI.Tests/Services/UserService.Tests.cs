using Xunit;
using Deeplex.Saverwalter.ModelTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FakeItEasy;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.Model.Auth;
using System.Text;

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

            var result = await userService.CreateUserAccount("test");

            result.Should().BeOfType<UserAccount>();
            result.Username.Should().Be("test");
        }

        [Fact]
        public async void SignInAsync()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var account = await userService.CreateUserAccount("test");
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));

            var result = await userService.SignInAsync("test", "test");

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
            var account = await userService.CreateUserAccount("test");
            await userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("test"));

            var result = await userService.SignInAsync("test", "test2");

            result.Succeeded.Should().Be(true);
            result.Account.Should().Be(account);
            result.SessionToken.Should().NotBeNullOrEmpty();
        }
    }
}