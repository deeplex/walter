using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model.Auth
{
    public class Pbkdf2PasswordCredential
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public virtual UserAccount User { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        [MaxLength(32)]
        public byte[] Salt { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [MaxLength(64)]
        public byte[] PasswordHash { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public int Iterations { get; set; }
    }
}
