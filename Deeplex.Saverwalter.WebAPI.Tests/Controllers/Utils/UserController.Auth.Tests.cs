// Copyright (c) 2026

using System.Security.Claims;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.ModelTests;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static Deeplex.Saverwalter.WebAPI.Controllers.Utils.UserController;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class UserControllerAuthTests : IDisposable
    {
        private readonly SaverwalterContext _ctx;
        private readonly TokenService _tokenService;
        private readonly UserService _userService;

        public UserControllerAuthTests()
        {
            _ctx = TestUtils.GetContext();
            _tokenService = new TokenService();
            _userService = new UserService(_ctx, _tokenService);
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }

        [Fact]
        public async Task RefreshTokenReturnsTokenForAuthenticatedUser()
        {
            var account = await _userService.CreateUserAccount("refresh-user", "Refresh User");
            var controller = BuildControllerForUser(account.Id);

            var result = await controller.RefreshToken();

            result.Value.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RefreshTokenReturnsUnauthorizedForUnknownUser()
        {
            var controller = BuildControllerForUser(Guid.NewGuid());

            var result = await controller.RefreshToken();

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task RefreshTokenReturnsUnauthorizedWhenClaimMissing()
        {
            var controller = BuildControllerWithPrincipal(new ClaimsPrincipal(new ClaimsIdentity()));

            var result = await controller.RefreshToken();

            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UpdatePasswordReturnsUnauthorizedWhenNoCredential()
        {
            var account = await _userService.CreateUserAccount("pwd-no-credential", "Pwd No Credential");
            var controller = BuildControllerForUser(account.Id);

            var result = await controller.UpdatePassword(new UpdatePasswordRequest
            {
                OldPassword = "old",
                NewPassword = "new"
            });

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UpdatePasswordReturnsBadRequestForWrongOldPassword()
        {
            var account = await _userService.CreateUserAccount("pwd-wrong-old", "Pwd Wrong Old");
            await _userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("correct-old"));

            var controller = BuildControllerForUser(account.Id);
            var result = await controller.UpdatePassword(new UpdatePasswordRequest
            {
                OldPassword = "wrong-old",
                NewPassword = "new-password"
            });

            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UpdatePasswordUpdatesCredentials()
        {
            var account = await _userService.CreateUserAccount("pwd-update", "Pwd Update");
            await _userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes("old-password"));
            var controller = BuildControllerForUser(account.Id);

            var result = await controller.UpdatePassword(new UpdatePasswordRequest
            {
                OldPassword = "old-password",
                NewPassword = "new-password"
            });

            result.Should().BeOfType<OkResult>();

            var signIn = await _userService.SignInAsync("pwd-update", "new-password");
            signIn.Succeeded.Should().BeTrue();
        }

        private UserController BuildControllerForUser(Guid userId)
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                    "TestAuth"));
            return BuildControllerWithPrincipal(principal);
        }

        private UserController BuildControllerWithPrincipal(ClaimsPrincipal principal)
        {
            var controller = new UserController(_ctx, _tokenService, _userService, A.Fake<HttpClient>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                }
            };
            return controller;
        }
    }
}
