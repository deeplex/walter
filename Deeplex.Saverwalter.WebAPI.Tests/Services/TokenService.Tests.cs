// Copyright (c) 2026

using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void CreateAndParseRoundTrip()
        {
            var sut = new TokenService();
            var expectedId = Guid.NewGuid();
            var account = new UserAccount
            {
                Id = expectedId,
                Username = "token-roundtrip",
                Name = "Token Roundtrip",
                Role = UserRole.User
            };

            var token = sut.CreateTokenFor(account);
            var ok = sut.TryParseToken(token, out var parsed);

            ok.Should().BeTrue();
            parsed.AccountId.Should().Be(expectedId);
        }

        [Fact]
        public void TryParseTokenRejectsMalformedToken()
        {
            var sut = new TokenService();

            var ok = sut.TryParseToken("not-a-valid-base64-token", out _);

            ok.Should().BeFalse();
        }

        [Fact]
        public void TryParseTokenRejectsTamperedToken()
        {
            var sut = new TokenService();
            var account = new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "token-tamper",
                Name = "Token Tamper",
                Role = UserRole.User
            };

            var token = sut.CreateTokenFor(account);
            var tampered = Convert.FromBase64String(token);
            tampered[tampered.Length - 1] ^= 0x01;

            var ok = sut.TryParseToken(Convert.ToBase64String(tampered), out _);

            ok.Should().BeFalse();
        }
    }
}
