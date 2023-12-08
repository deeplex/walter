using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.Model.Auth
{
    [Index(nameof(Token), IsUnique = true)]
    public class UserResetCredential
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }
        [Required]
        public virtual UserAccount User { get; set; } = default!; // See https://github.com/dotnet/efcore/issues/12078

        [Required]
        [MaxLength(16)]
        public byte[] Token { get; set; } = default!;
        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
