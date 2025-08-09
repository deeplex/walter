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
using Deeplex.Saverwalter.Model.Auth;

namespace Deeplex.Saverwalter.Model
{
    public class Kontakt
    {
        public string Bezeichnung => string.IsNullOrEmpty(Vorname) ? Name : $"{Vorname} {Name}";

        public int KontaktId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Rechtsform Rechtsform { get; set; }
        public string? Vorname { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }

        // Only valid if Rechtsform != natuerlich
        public virtual List<Kontakt> JuristischePersonen { get; set; } = [];
        public virtual List<Kontakt> Mitglieder { get; set; } = [];

        public virtual List<Vertrag> VerwaltetVertraege { get; private set; } = [];
        public virtual List<Vertrag> Mietvertraege { get; private set; } = [];
        public virtual List<Wohnung> Wohnungen { get; private set; } = [];
        public virtual List<Garage> Garagen { get; private set; } = [];
        public virtual List<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; private set; } = [];
        public virtual List<UserAccount> Accounts { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Kontakt(string name, Rechtsform rechtsform)
        {
            Name = name;
            Rechtsform = rechtsform;
        }
    }

    public enum Anrede
    {
        Herr,
        Frau,
        Keine,
    }

    public enum Titel
    {
        Kein,
        Doktor,
    }

    public enum Rechtsform
    {
        [Description("Natürliche Person")]
        natuerlich,
        [Description("GmbH")]
        gmbh,
        [Description("GbR")]
        gbr,
        [Description("AG")]
        ag,
        [Description("e.V.")]
        ev,
        [Description("KG")]
        kg,
        [Description("OHG")]
        ohg,
        [Description("UG (haftungsbeschränkt)")]
        ug,
        [Description("Stiftung")]
        stiftung,
        [Description("Verein")]
        verein,
        [Description("Genossenschaft")]
        genossenschaft,
        [Description("Sonstige")]
        sonstige,
    }
}
