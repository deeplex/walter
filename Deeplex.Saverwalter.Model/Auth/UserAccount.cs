using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Deeplex.Saverwalter.Model.Auth
{
    [Index(nameof(Username), IsUnique = true)]
    public class UserAccount
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078 

        public virtual Pbkdf2PasswordCredential? Pbkdf2PasswordCredential { get; set; }

        public Claim[] AssembleClaims()
        {
            return new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Id.ToString("D", CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, Username),
            };
        }

    }
}
