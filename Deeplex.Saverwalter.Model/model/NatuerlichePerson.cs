using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public sealed class NatuerlichePerson : IPerson
    {
        public string Bezeichnung => string.Join(" ", Vorname ?? "", Nachname);

        public Guid PersonId { get; set; }
        public int NatuerlichePersonId { get; set; }
        public string? Vorname { get; set; }
        public string Nachname { get; set; } = null!;
        public Titel Titel { get; set; }
        public bool isVermieter { get; set; }
        public bool isMieter { get; set; }
        public bool isHandwerker { get; set; }
        public Anrede Anrede { get; set; }
        public string? Telefon { get; set; }
        public string? Mobil { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public int? AdresseId { get; set; }
        public Adresse? Adresse { get; set; }
        public List<JuristischePersonenMitglied> JuristischePersonen { get; private set; } = new List<JuristischePersonenMitglied>();
        public string? Notiz { get; set; }

        public NatuerlichePerson()
        {
            PersonId = Guid.NewGuid();
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
}
