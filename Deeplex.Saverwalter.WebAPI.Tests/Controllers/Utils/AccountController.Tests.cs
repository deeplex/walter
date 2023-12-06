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
            using (var ctx = TestUtils.GetContext())
            {
                var tokenService = A.Fake<TokenService>();
                var userService = new UserService(ctx, tokenService);
                var controller = new AccountController(ctx, tokenService, userService);

                var result = await controller.Create(new CreateRequest
                {
                    Username = "create_user_test",
                    Name = "Mister Test",
                    Password = "test"
                });

                result.Should().BeOfType<OkResult>();
                ctx.UserAccounts.Last().Username.Should().Be("create_user_test");
            }
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
                Username = "test1",
                Name = "Miss Test",
                Password = "test"
            });

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
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

            var _ = await controller.Create(new CreateRequest
            {
                Username = "test2",
                Password = "test"
            });

            var result = await controller.SignIn(new SignInRequest
            {
                Username = "test2",
                Password = "test2"
            });

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
