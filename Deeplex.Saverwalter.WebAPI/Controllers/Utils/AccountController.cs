using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserService userService;

        public AccountController(UserService userService)
        {
            this.userService = userService;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
        public class LoginResult
        {
            public string? UserId { get; set; }
            public string? Token { get; set; }
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] LoginRequest loginRequest)
        {
            var result = await userService.SignInAsync(loginRequest.Username, loginRequest.Password);
            if (result.Succeeded)
            {
                return Ok(new LoginResult
                {
                    Token = result.SessionToken,
                    UserId = result.Account!.Id.ToString("D", CultureInfo.InvariantCulture)
                });
            }
            return Unauthorized();
        }
    }
}
