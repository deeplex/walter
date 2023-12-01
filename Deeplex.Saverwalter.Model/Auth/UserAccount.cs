using Deeplex.Saverwalter.Model.model;
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

        public string Name { get; set; } = default!;

        public virtual Pbkdf2PasswordCredential? Pbkdf2PasswordCredential { get; set; }
        public virtual UserResetCredential? UserResetCredential { get; set; }

        public Claim[] AssembleClaims()
        {
            return
            [
                new Claim(ClaimTypes.NameIdentifier, Id.ToString("D", CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, Username),
            ];
        }
    }
}
