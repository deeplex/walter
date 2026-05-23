// Copyright (c) 2023-2026 Kai Lawrence
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
    public class GarageVertrag
    {
        public int GarageVertragId { get; set; }
        [Required]
        public virtual Garage Garage { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public virtual Vertrag? Vertrag { get; set; }
        public DateOnly? Ende { get; set; }
        public string? Notiz { get; set; }

        public virtual Buchungskonto MietBuchungskonto { get; set; } = null!;
        public virtual Buchungskonto ZahlungsKonto { get; set; } = null!;

        public virtual List<GarageVertragVersion> Versionen { get; private set; } = [];
        public virtual List<Kontakt> Mieter { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public GarageVertrag()
        {
        }

        public DateOnly Beginn() => Versionen.Count > 0
            ? Versionen.Min(v => v.Beginn)
            : DateOnly.MinValue;
    }
}
