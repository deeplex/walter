// Copyright (c) 2023-2024 Kai Lawrence
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
using Deeplex.Saverwalter.Model.Auth;

namespace Deeplex.Saverwalter.Model
{
    public class Verwalter
    {
        public int VerwalterId { get; set; }
        [Required]
        public virtual UserAccount UserAccount { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public VerwalterRolle Rolle { get; set; }
        public string? Notiz { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Verwalter(VerwalterRolle rolle)
        {
            Rolle = rolle;
        }
    }

    // Je higher die Rolle, desto mehr Rechte hat der Verwalter
    public enum VerwalterRolle
    {
        Keine,
        Vollmacht,
        Eigentuemer
    }
}
