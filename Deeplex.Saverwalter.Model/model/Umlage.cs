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

namespace Deeplex.Saverwalter.Model
{
    public class Umlage
    {
        public int UmlageId { get; set; }
        public virtual Umlagetyp Typ { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public string? Beschreibung { get; set; }
        public string? Notiz { get; set; }

        /// <summary>HKVOs, for which this Umlage IS the Heizkosten component.</summary>
        public virtual List<HKVO> HeizkostenHKVOs { get; set; } = [];
        /// <summary>HKVOs, for which this Umlage IS the Betriebsstrom component.</summary>
        public virtual List<HKVO> BetriebsstromHKVOs { get; set; } = [];

        public virtual Buchungskonto NkVerrechnungsKonto { get; set; } = null!;
        public virtual Buchungskonto ZahlungsKonto { get; set; } = null!;

        public virtual List<UmlageVersion> Versionen { get; private set; } = [];
        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Zaehler> Zaehler { get; set; } = new List<Zaehler>();

        public DateOnly? Ende { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Umlage() { }
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
