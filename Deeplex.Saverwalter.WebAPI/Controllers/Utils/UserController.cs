﻿using System.Globalization;
using System.Security.Claims;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AccountController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly SaverwalterContext _dbContext;
        private readonly TokenService _tokenService;
        private readonly UserService _userService;

        public UserController(SaverwalterContext dbContext, TokenService tokenService, UserService userService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _userService = userService;
        }

        public class SignInRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
        public class LoginResult
        {
            public string UserId { get; }
            public string Token { get; }

            public LoginResult(string userId, string token)
            {
                UserId = userId;
                Token = token;
            }
        }

        [HttpGet]
        public IActionResult Get() => _userService.Get(User!);

        [HttpPut]
        public IActionResult Put(AccountEntryBase entry) => _userService.Put(User!, entry);

        [HttpDelete]
        public IActionResult Delete() => _userService.Delete(User!);

        [HttpPost("refresh-token")]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var user = User;
            if (user == null) // this is always false if aspnet core is properly configured
            {
                return Unauthorized();
            }
            var userIdClaim = user.Claims.Single((claim) => claim.Type == ClaimTypes.NameIdentifier);
            var account = await _userService.GetUserById(Guid.Parse(userIdClaim.Value));
            if (account == null)
            {
                return Unauthorized();
            }

            return _tokenService.CreateTokenFor(account);
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResult>> SignIn([FromBody] SignInRequest loginRequest)
        {
            var result = await _userService.SignInAsync(loginRequest.Username, loginRequest.Password);
            if (result.Succeeded &&
                result.SessionToken is string token &&
                result.Account is Model.Auth.UserAccount account)
            {
                var userId = account.Id.ToString("D", CultureInfo.InvariantCulture);
                return new LoginResult(userId, token);
            }
            return Unauthorized();
        }

        public class UpdatePasswordRequest
        {
            public string OldPassword { get; set; } = null!;
            public string NewPassword { get; set; } = null!;
        }

        [HttpPost("update-password")]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            var user = User;
            if (user == null) // this is always false if aspnet core is properly configured
            {
                return Unauthorized();
            }
            var userIdClaim = user.Claims.Single((claim) => claim.Type == ClaimTypes.NameIdentifier);
            var account = await _userService.GetUserById(Guid.Parse(userIdClaim.Value));
            if (account?.Pbkdf2PasswordCredential == null)
            {
                return Unauthorized();
            }

            if (!_userService.ValidatePbkdf2PasswordCredentials(
                account.Pbkdf2PasswordCredential,
                Encoding.UTF8.GetBytes(updatePasswordRequest.OldPassword)))
            {
                return BadRequest();
            }

            await _userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes(updatePasswordRequest.NewPassword));
            return Ok();
        }
    }
}
