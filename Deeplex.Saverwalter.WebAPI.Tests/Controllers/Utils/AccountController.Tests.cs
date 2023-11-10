using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.AccountController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public async void CreateUser()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

            var result = await controller.Create(new CreateRequest
            {
                Username = "test",
                Password = "test"
            });

            result.Should().BeOfType<OkResult>();
            ctx.UserAccounts.Should().HaveCount(1);
            ctx.UserAccounts.First().Username.Should().Be("test");
        }

        [Fact]
        public async void SignInSuccess()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

            var _ = await controller.Create(new CreateRequest
            {
                Username = "test",
                Password = "test"
            });

            var result = await controller.SignIn(new SignInRequest
            {
                Username = "test",
                Password = "test"
            });

            result.Value.Should().BeOfType<LoginResult>();
        }

        [Fact]
        public async void SignInFailed()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

            var _ = await controller.Create(new CreateRequest
            {
                Username = "test",
                Password = "test"
            });

            var result = await controller.SignIn(new SignInRequest
            {
                Username = "test",
                Password = "test2"
            });

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}