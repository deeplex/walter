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
    public class Wohnung
    {
        public int WohnungId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }
        [Required]
        public double Wohnflaeche { get; set; }
        [Required]
        public double Nutzflaeche { get; set; }
        [Required]
        public double Miteigentumsanteile { get; set; }
        [Required]
        public int Nutzeinheit { get; set; } // TODO Rename to Nutzeinheiten
        public virtual Kontakt? Besitzer { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Vertrag> Vertraege { get; private set; } = [];
        public virtual List<Zaehler> Zaehler { get; private set; } = [];
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = [];
        public virtual List<Umlage> Umlagen { get; private set; } = [];
        public virtual List<Verwalter> Verwalter { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Wohnung(string bezeichnung, double wohnflaeche, double nutzflaeche, double miteigentumsanteile, int nutzeinheit)
        {
            Bezeichnung = bezeichnung;
            Wohnflaeche = wohnflaeche;
            Nutzflaeche = nutzflaeche;
            Miteigentumsanteile = miteigentumsanteile;
            Nutzeinheit = nutzeinheit;
        }
    }
}
