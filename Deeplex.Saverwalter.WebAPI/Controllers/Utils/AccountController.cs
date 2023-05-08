using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly SaverwalterContext _dbContext;
        private readonly TokenService _tokenService;
        private readonly UserService _userService;

        public AccountController(SaverwalterContext dbContext, TokenService tokenService, UserService userService)
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
            public string UserId { get; set; }
            public string Token { get; set; }
        }

        [HttpPost("refresh-token")]
        [Produces("text/plain")]
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
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResult>> SignIn([FromBody] SignInRequest loginRequest)
        {
            var result = await _userService.SignInAsync(loginRequest.Username, loginRequest.Password);
            if (result.Succeeded)
            {
                return new LoginResult
                {
                    Token = result.SessionToken!,
                    UserId = result.Account!.Id.ToString("D", CultureInfo.InvariantCulture)
                };
            }
            return Unauthorized();
        }

        public class CreateRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        [HttpPost("create")]
        [ProducesErrorResponseType(typeof(void))] // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Create([FromBody] CreateRequest createRequest)
        {
            // either create the account _and_ associate a password or do nothing
            using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var account = await _userService.CreateUserAccount(createRequest.Username);
                await _userService.UpdateUserPassword(account, Encoding.UTF8.GetBytes(createRequest.Password));

                await tx.CommitAsync();
                return Ok();
            }
            catch (DbUpdateException)
            {
                return Conflict();
            }
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
