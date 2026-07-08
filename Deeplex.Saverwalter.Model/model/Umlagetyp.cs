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
    /// <summary>
    /// Kanonische Betriebskostenkategorien nach §2 BetrKV.
    /// Wert entspricht der offiziellen Nummer (Nr. 1–14).
    /// </summary>
    public enum BetrKVNummer
    {
        Nr1_OeffentlicheLasten = 1,
        Nr2_Wasserversorgung = 2,
        Nr3_Entwaesserung = 3,
        Nr4_Fahrstuhl = 4,
        Nr5_StrassenreinigungUndMuellbeseitigung = 5,
        Nr6_GebaeudereinigungUndUngezieferbekaempfung = 6,
        Nr7_Gartenpflege = 7,
        Nr8_Beleuchtung = 8,
        Nr9_Schornsteinreinigung = 9,
        Nr10_SachUndHaftpflichtversicherung = 10,
        Nr11_Hauswart = 11,
        Nr12_AntenneOderBreitband = 12,
        Nr13_MaschinelleWascheinrichtung = 13,
        Nr14_SonstigeBetriebskosten = 14,
    }

    public class Umlagetyp
    {
        public int UmlagetypId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }

        public BetrKVNummer? BetrKVNummer { get; set; }

        public string? Notiz { get; set; }

        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Umlagetyp(string bezeichnung)
        {
            Bezeichnung = bezeichnung;
        }
    }
}

