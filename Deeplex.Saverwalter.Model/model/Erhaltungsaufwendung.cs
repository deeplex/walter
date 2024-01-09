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
    public class Erhaltungsaufwendung
    {
        public int ErhaltungsaufwendungId { get; set; }
        [Required]
        public DateOnly Datum { get; set; }
        [Required]
        public virtual Kontakt Aussteller { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public string Bezeichnung { get; set; }
        [Required]
        public double Betrag { get; set; }
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        public string? Notiz { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Erhaltungsaufwendung(double betrag, string bezeichnung, DateOnly datum)
        {
            Betrag = betrag;
            Bezeichnung = bezeichnung;
            Datum = datum;
        }
    }
}
