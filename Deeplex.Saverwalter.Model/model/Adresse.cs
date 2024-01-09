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
    public class Adresse
    {
        public string Anschrift => string.Join(", ",
            string.Join(" ", Strasse, Hausnummer),
            string.Join(" ", Postleitzahl, Stadt));

        public int AdresseId { get; set; }
        [Required]
        public string Hausnummer { get; set; }
        [Required]
        public string Strasse { get; set; }
        [Required]
        public string Postleitzahl { get; set; }
        [Required]
        public string Stadt { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Wohnung> Wohnungen { get; set; } = new();
        public virtual List<Garage> Garagen { get; private set; } = new();
        public virtual List<Kontakt> Kontakte { get; private set; } = new();
        public virtual List<Zaehler> Zaehler { get; private set; } = new();

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Adresse(string strasse, string hausnummer, string postleitzahl, string stadt)
        {
            Strasse = strasse;
            Hausnummer = hausnummer;
            Postleitzahl = postleitzahl;
            Stadt = stadt;
        }
    }
}
