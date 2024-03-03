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
using System.Security.Cryptography;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AccountController;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class SignInResult
    {
        public bool Succeeded { get; set; }

        public UserAccount? Account { get; set; }
        public string? SessionToken { get; set; }
    }

    public class UserService
    {
        private const int maxSessionDurationD = 7;
        private const int maxSessionsPerUser = 128;

        private const int hashSize = 64;
        private const int defaultIterations = 210000;
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private readonly SaverwalterContext walterContext;
        private readonly TokenService tokenService;

        public ActionResult<AccountEntryBase> Get(ClaimsPrincipal user)
        {
            var name = user.Identity?.Name;
            var entity = walterContext.UserAccounts.Single(e => e.Username == name);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                return new AccountEntryBase(entity);
            }
            catch
            {
                return new BadRequestResult();
            }
        }


        public ActionResult<AccountEntryBase> Put(ClaimsPrincipal user, AccountEntryBase entry)
        {
            var name = user.Identity?.Name;
            var entity = walterContext.UserAccounts.Single(e => e.Name == name);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                return Update(entry, entity);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private AccountEntryBase Update(AccountEntryBase entry, UserAccount entity)
        {
            entity.Name = entry.Name;

            walterContext.UserAccounts.Update(entity);
            walterContext.SaveChanges();

            return new AccountEntryBase(entity);
        }

        public ActionResult Delete(ClaimsPrincipal user)
        {
            var name = user.Identity?.Name;
            var entity = walterContext.UserAccounts.Single(e => e.Name == name);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            walterContext.UserAccounts.Remove(entity);
            walterContext.SaveChanges();

            return new OkResult();
        }

        public UserService(SaverwalterContext walterContext, TokenService tokenService)
        {
            this.walterContext = walterContext;
            this.tokenService = tokenService;
        }

        public ValueTask<UserAccount?> GetUserById(Guid id)
        {
            return walterContext.UserAccounts.FindAsync(id);
        }

        public async Task<SignInResult> SignInAsync(string username, string password)
        {
            var account = await walterContext.UserAccounts
                 .Where((account) => account.Username == username)
                 .Include((account) => account.Pbkdf2PasswordCredential)
                 .FirstOrDefaultAsync();
            if (account?.Pbkdf2PasswordCredential == null)
            {
                return new SignInResult { Succeeded = false };
            }
            if (!ValidatePbkdf2PasswordCredentials(account.Pbkdf2PasswordCredential, Encoding.UTF8.GetBytes(password)))
            {
                return new SignInResult { Succeeded = false };
            }
            var token = tokenService.CreateTokenFor(account);

            return new SignInResult { Succeeded = true, Account = account, SessionToken = token };
        }

        public async Task<UserAccount> CreateUserAccount(string username, string name)
        {
            var account = new UserAccount { Username = username, Name = name };
            walterContext.UserAccounts.Add(account);
            await walterContext.SaveChangesAsync();
            return account;
        }

        public void SetUserPassword(UserAccount account, string password) => SetUserPassword(account, Encoding.UTF8.GetBytes(password));
        public void SetUserPassword(UserAccount account, byte[] utf8Password)
        {
            var credential = account.Pbkdf2PasswordCredential;
            if (credential == null)
            {
                credential = new Pbkdf2PasswordCredential
                {
                    User = account,
                };
                account.Pbkdf2PasswordCredential = credential;
                walterContext.Pbkdf2PasswordCredentials.Add(credential);
            }

            credential.Iterations = defaultIterations;
            credential.Salt = RandomNumberGenerator.GetBytes(32);
            credential.PasswordHash = HashPassword(utf8Password, credential.Salt, credential.Iterations);
        }
        public async Task UpdateUserPassword(UserAccount account, byte[] utf8Password)
        {
            SetUserPassword(account, utf8Password);
            await walterContext.SaveChangesAsync();
        }

        public bool ValidatePbkdf2PasswordCredentials(Pbkdf2PasswordCredential credential, byte[] utf8Password)
        {
            var passwordHash = HashPassword(utf8Password, credential.Salt, credential.Iterations);
            // TODO: Sleep(CSPRNG.random(0, 200))
            return ByteEquals(credential.PasswordHash, passwordHash);
        }

        public async Task<string> CreateResetToken(UserAccount user)
        {
            var token = RandomNumberGenerator.GetBytes(16);
            user.UserResetCredential = new UserResetCredential
            {
                User = user,
                Token = token,
                ExpiresAt = DateTime.Now.AddDays(7).AsUtcKind(), // TODO: Make this configurable
            };
            walterContext.UserResetCredentials.Add(user.UserResetCredential);
            await walterContext.SaveChangesAsync();

            return WebEncoders.Base64UrlEncode(token);
        }
        public async Task<SignInResult> TryRedeemResetToken(string resetToken, string newUserPassword)
        {
            byte[] decodedToken;
            try
            {
                decodedToken = WebEncoders.Base64UrlDecode(resetToken);
            }
            catch (FormatException)
            {
                return new SignInResult { Succeeded = false };
            }

            var credential = await walterContext.UserResetCredentials
                .Where(cred => cred.Token == decodedToken)
                .Include(cred => cred.User)
                    .ThenInclude(user => user.Pbkdf2PasswordCredential)
                .SingleOrDefaultAsync();

            if (credential == null)
            {
                return new SignInResult { Succeeded = false };
            }
            try
            {
                if (credential.ExpiresAt < DateTime.Now)
                {
                    return new SignInResult { Succeeded = false };
                }
                SetUserPassword(credential.User, newUserPassword);
                var sessionToken = tokenService.CreateTokenFor(credential.User);
                return new SignInResult
                {
                    Succeeded = true,
                    Account = credential.User,
                    SessionToken = sessionToken,
                };
            }
            finally
            {
                credential.User.UserResetCredential = null;
                walterContext.Remove(credential);
                await walterContext.SaveChangesAsync();
            }
        }

        private static byte[] HashPassword(byte[] password, byte[] salt, int iterations)
            => Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, hashSize);
        private static bool ByteEquals(ReadOnlySpan<byte> lhs, ReadOnlySpan<byte> rhs)
            => lhs.SequenceEqual(rhs);
    }
}
