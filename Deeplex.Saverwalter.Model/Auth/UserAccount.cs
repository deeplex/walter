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
