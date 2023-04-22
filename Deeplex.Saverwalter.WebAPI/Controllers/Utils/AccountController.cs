using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(TokenService tokenService, UserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = tokenService;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public class LoginRequest
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
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) // this is always false if aspnet core is properly configured
            {
                return Unauthorized();
            }
            var userIdClaim = user.Claims.Single((claim) => claim.ValueType == ClaimTypes.NameIdentifier);
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
        public async Task<ActionResult<LoginResult>> SignIn([FromBody] LoginRequest loginRequest)
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
    }
}
