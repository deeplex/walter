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

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Deeplex.Saverwalter.Model.Auth;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public struct TokenInfo
    {
        public Guid AccountId { get; set; }
    }

    public class TokenService
    {
        private readonly byte[] authKey;
        private readonly TimeSpan maxTokenLifetime = new TimeSpan(7, 0, 0, 0, 0);

        private const int tokenSaltOffset = 0;
        private const int tokenSaltSize = 16;
        private const int tokenInfoOffset = tokenSaltOffset + tokenSaltSize;
        private const int tokenInfoSize = 16;
        private const int additionalDataOffset = tokenInfoOffset + tokenInfoSize;
        private const int additionalDataSize = 16;
        private const int tagOffset = additionalDataOffset + additionalDataSize;
        private const int tagSize = 16;
        private const int tokenSize = tagOffset + tagSize;

        public TokenService()
        {
            // can be pushed to DB if vertical scaling becomes necessary
            authKey = RandomNumberGenerator.GetBytes(64);
        }

        public string CreateTokenFor(UserAccount account)
        {
            var tokenSalt = RandomNumberGenerator.GetBytes(tokenSaltSize);
            var keyNonce = HKDF.Extract(HashAlgorithmName.SHA384, authKey, tokenSalt);
            using var key = new AesGcm(keyNonce.AsSpan(0, 32), tagSize);
            var nonce = keyNonce.AsSpan(32, 12);

            var expiry = DateTime.UtcNow + maxTokenLifetime;
            var tokenInfo = account.Id.ToByteArray();

            var token = new byte[tokenSize];
            tokenSalt.CopyTo(token.AsSpan(0, tokenSaltSize));
            var ciphertext = token.AsSpan(tokenInfoOffset, tokenInfoSize);
            var additionalData = token.AsSpan(additionalDataOffset, additionalDataSize);
            Encoding.UTF8.GetBytes(
                    expiry.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture),
                    additionalData
            );
            var tokenTag = token.AsSpan(tagOffset);

            key.Encrypt(nonce, tokenInfo, ciphertext, tokenTag, additionalData);
            return Convert.ToBase64String(token);
        }

        public bool TryParseToken(string b64Token, out TokenInfo tokenInfo)
        {
            tokenInfo = new TokenInfo();
            var token = new byte[tokenSize];
            if (!Convert.TryFromBase64String(b64Token, token, out var realTokenSize)
                || realTokenSize != tokenSize)
            {
                return false;
            }

            var tokenSalt = token.AsSpan(0, tokenSaltSize);
            var keyNonce = new byte[384 / 8];
            HKDF.Extract(HashAlgorithmName.SHA384, authKey, tokenSalt, keyNonce);
            using var key = new AesGcm(keyNonce.AsSpan(0, 32), tagSize);
            var nonce = keyNonce.AsSpan(32, 12);

            var ciphertext = token.AsSpan(tokenInfoOffset, tokenInfoSize);
            var additionalData = token.AsSpan(additionalDataOffset, additionalDataSize);
            var tokenTag = token.AsSpan(tagOffset);

            var rawTokenInfo = new byte[tokenInfoSize];
            try
            {
                key.Decrypt(nonce, ciphertext, tokenTag, rawTokenInfo, additionalData);
            }
            catch (CryptographicException)
            {
                return false;
            }

            if (!DateTime.TryParseExact(
                    Encoding.UTF8.GetString(additionalData),
                    "yyyyMMddTHHmmssZ",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var expiry)
                || expiry < DateTime.UtcNow)
            {
                return false;
            }

            tokenInfo.AccountId = new Guid(rawTokenInfo);
            return true;
        }
    }
}
