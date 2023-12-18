using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.WebAPI.Helper
{
    internal static class AccountExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindAll(ClaimTypes.NameIdentifier).Single();
            return Guid.Parse(id.Value);
        }
    }
}
