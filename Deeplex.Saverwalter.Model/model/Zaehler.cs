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
    public class Zaehler
    {
        public int ZaehlerId { get; set; }
        [Required]
        public string Kennnummer { get; set; }
        [Required]
        public Zaehlertyp Typ { get; set; }
        public virtual Wohnung? Wohnung { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public virtual DateOnly? Ende { get; set; }
        public string? Notiz { get; set; }

        public virtual List<Zaehlerstand> Staende { get; private set; } = new List<Zaehlerstand>();
        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Zaehler(string kennnummer, Zaehlertyp typ)
        {
            Kennnummer = kennnummer;
            Typ = typ;
        }
    }

    public enum Zaehlertyp
    {
        [Unit(Zaehlereinheit.Kubikmeter)]
        Warmwasser,
        [Unit(Zaehlereinheit.Kubikmeter)]
        Kaltwasser,
        [Unit(Zaehlereinheit.Kilowattstunden)]
        Strom,
        [Unit(Zaehlereinheit.Kilowattstunden)]
        Gas,
    }

    public enum Zaehlereinheit
    {
        [UnitString("")]
        Dimensionslos,
        [UnitString("m³")]
        Kubikmeter,
        [UnitString("kWh")]
        Kilowattstunden
    }
}
