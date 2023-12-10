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
            var controller = new UserController(ctx, tokenService, userService);

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
            var controller = new UserController(ctx, tokenService, userService);

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
