using Deeplex.Saverwalter.WebAPI.Services;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Sprache;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Text;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        public class LoginEntry
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Token { get; set; }

            public LoginEntry() { }
        }

        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] LoginEntry entry)
        {
            if (entry.Token != null)
            {
                var tokenValid = BasicAuthentication.ValidateAccessToken(entry.Token);
                return new OkObjectResult(new { succeeded = tokenValid });
            }

            var result = BasicAuthentication.Authenticate(entry.Username!, entry.Password!, "Basic");
            if (result.Succeeded)
            {
                var token = BasicAuthentication.GetJwtSecurityToken(result.Ticket);
                return new OkObjectResult(new { succeeded = result.Succeeded, accessToken = token });
            }
            else
            {
                return new BadRequestObjectResult(result);
            }
        }
    }
}
