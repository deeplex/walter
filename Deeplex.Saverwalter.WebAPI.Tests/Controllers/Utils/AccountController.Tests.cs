using System.Text;
using Deeplex.Saverwalter.Model.Auth;
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
        public async void SignInSuccess()
        {
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

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
            var ctx = TestUtils.GetContext();
            var tokenService = A.Fake<TokenService>();
            var userService = new UserService(ctx, tokenService);
            var controller = new AccountController(ctx, tokenService, userService);

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
                Username = "test2",
                Password = "test2"
            });

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
