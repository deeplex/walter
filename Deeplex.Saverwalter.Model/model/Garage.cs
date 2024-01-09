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

namespace Deeplex.Saverwalter.Model
{
    public class Garage
    {
        public int GarageId { get; set; }
        [Required]
        public string Kennung { get; set; }
        [Required]
        public virtual Kontakt Besitzer { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }
        public virtual List<Vertrag> Vertraege { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Garage(string kennung)
        {
            Kennung = kennung;
        }
    }
}
