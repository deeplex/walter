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
