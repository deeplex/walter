using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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

        public UserService(SaverwalterContext walterContext, TokenService tokenService)
        {
            this.walterContext = walterContext;
            this.tokenService = tokenService;
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

        public async Task<UserAccount> CreateUserAccount(string username)
        {
            var account = new UserAccount { Username = username };
            walterContext.UserAccounts.Add(account);
            await walterContext.SaveChangesAsync();
            return account;
        }
        public async Task UpdateUserPassword(UserAccount account, byte[] utf8Password)
        {
            var credential = new Pbkdf2PasswordCredential
            {
                User = account,
                Iterations = defaultIterations,
                Salt = RandomNumberGenerator.GetBytes(32),
            };
            credential.PasswordHash = HashPassword(utf8Password, credential.Salt, credential.Iterations);
            account.Pbkdf2PasswordCredential = credential;
            walterContext.Pbkdf2PasswordCredentials.Add(credential);
            await walterContext.SaveChangesAsync();
        }

        private bool ValidatePbkdf2PasswordCredentials(Pbkdf2PasswordCredential credential, byte[] utf8Password)
        {
            var passwordHash = HashPassword(utf8Password, credential.Salt, credential.Iterations);
            // TODO: Sleep(CSPRNG.random(0, 200))
            return ByteEquals(credential.PasswordHash, passwordHash);
        }

        private static byte[] HashPassword(byte[] password, byte[] salt, int iterations)
            => Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, hashSize);
        private static bool ByteEquals(ReadOnlySpan<byte> lhs, ReadOnlySpan<byte> rhs)
            => lhs.SequenceEqual(rhs);
    }
}
