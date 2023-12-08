using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.Model.Auth
{
    [Index(nameof(Username), IsUnique = true)]
    public class UserAccount
    {
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078 
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public UserRole Role { get; set; }

        public string? Email { get; set; }

        public virtual List<Verwalter> Verwalter { get; set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public virtual Pbkdf2PasswordCredential? Pbkdf2PasswordCredential { get; set; }
        public virtual UserResetCredential? UserResetCredential { get; set; }

        public Claim[] AssembleClaims()
        {
            return
            [
                new Claim(ClaimTypes.NameIdentifier, Id.ToString("D", CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, Role.ToString())
            ];
        }
    }

    public enum UserRole
    {
        Guest,
        User,
        Admin,
        Owner,
    }
}
