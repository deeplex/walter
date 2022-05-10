using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.Model
{
    // JuristischePerson is a Name. Kontakte may subscribe to this and is used for dashboards and stuff... nothing wild really.
    public sealed class JuristischePerson : IPerson
    {
        public Guid PersonId { get; set; }
        public int JuristischePersonId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
        public List<Garage> Garagen { get; private set; } = new List<Garage>();
        public List<JuristischePerson> JuristischeMitglieder { get; private set; } = new List<JuristischePerson>();
        public List<NatuerlichePerson> NatuerlicheMitglieder { get; private set; } = new List<NatuerlichePerson>();
        public List<JuristischePerson> JuristischePersonen { get; set; } = new List<JuristischePerson>();
        public string? Notiz { get; set; }
        public Anrede Anrede { get; set; }

        public List<IPerson> Mitglieder => JuristischeMitglieder.Select(w => (IPerson)w)
            .Concat(NatuerlicheMitglieder.Select(w => (IPerson)w)).ToList();

        public JuristischePerson()
        {
            PersonId = Guid.NewGuid();
        }
    }
}
