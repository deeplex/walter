using Deeplex.Saverwalter.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Model.model
{
    public class UserResetCredential
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public virtual UserAccount User { get; set; } = default!; // See https://github.com/dotnet/efcore/issues/12078

        public byte[] Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
}
