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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Umlage
    {
        public int UmlageId { get; set; }
        [Required]
        public virtual Umlagetyp Typ { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public Umlageschluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }
        public string? Notiz { get; set; }
        public virtual HKVO? HKVO { get; set; }

        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Betriebskostenrechnung> Betriebskostenrechnungen { get; private set; } = new List<Betriebskostenrechnung>();
        public virtual List<Zaehler> Zaehler { get; set; } = new List<Zaehler>();
        // Stromrechnungen keep track of the HKVOs that point to them
        public virtual List<HKVO> HKVOs { get; set; } = new List<HKVO>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Umlage(Umlageschluessel schluessel)
        {
            Schluessel = schluessel;
        }
    }

    public enum Umlageschluessel
    {
        [Description("n. WF")]
        NachWohnflaeche,
        [Description("n. NE")]
        NachNutzeinheit,
        [Description("n. Pers.")]
        NachPersonenzahl,
        [Description("n. Verb.")]
        NachVerbrauch,
        [Description("n. NF")]
        NachNutzflaeche,
        [Description("n. MEA")]
        NachMiteigentumsanteil
    }
}
