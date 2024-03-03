// Copyright (c) 2023-2024 Henrik S. Gaßmann, Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static readonly string HttpAuthScheme = "X-WalterToken ";

        public TokenService TokenService { get; }
        private SaverwalterContext SaverwalterContext { get; }

        public TokenAuthenticationHandler(TokenService tokenService, SaverwalterContext saverwalterContext, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            TokenService = tokenService;
            SaverwalterContext = saverwalterContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.Authorization.Count != 1)
            {
                return AuthenticateResult.NoResult();
            }
            var authorizationHeader = Request.Headers.Authorization[0]!;
            if (authorizationHeader == null || !authorizationHeader.StartsWith(HttpAuthScheme))
            {
                return AuthenticateResult.NoResult();
            }

            if (!TokenService.TryParseToken(authorizationHeader.Substring(HttpAuthScheme.Length), out var tokenInfo))
            {
                return AuthenticateResult.Fail("Invalid WalterToken");
            }

            var user = await SaverwalterContext.UserAccounts.FindAsync(tokenInfo.AccountId);
            if (user == null)
            {
                return AuthenticateResult.Fail("No matching user account found. Has it been deleted?");
            }

            var identity = new ClaimsIdentity(user.AssembleClaims(), Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
    }
}
