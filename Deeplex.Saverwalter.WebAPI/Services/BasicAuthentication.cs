using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sprache;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class BasicAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var accessToken = Request.Cookies["access_token"];

            if (accessToken == null)
            {
                return AuthenticateResult.Fail("No access token.");
            }

            var valid = ValidateAccessToken(accessToken);

            if (!valid)
            {
                return AuthenticateResult.Fail("Access token invalid or expired.");
            }

            // TODO replace admin with real username
            var claims = new[] { new Claim(ClaimTypes.Name, "admin") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private static bool ValidateAccessToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateLifetime = true
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, validationParameters, out var validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string signingKey = "VERYSECURESIGNINGKEY!!!";
        private static string issuer = "http://localhost:7251";
        private static string audience = "http://localhost:5173";

        public static string GetJwtSecurityToken(AuthenticationTicket ticket)
        {
            var signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKeyBytes), SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: ticket.Principal.Claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: signingCredentials);
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.WriteToken(jwt);

            return token;
        }

        public static AuthenticateResult Authenticate(string username, string password, string scheme)
        {
            if (!IsAuthenticated(username, password))
            {
                return AuthenticateResult.Fail("Invalid username or password.");
            }

            // Create a claims identity for the authenticated user
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, scheme);

            return AuthenticateResult.Success(ticket);
        }

        private static bool IsAuthenticated(string username, string password)
        {
            if (username == "admin" && password == "verysecure")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
