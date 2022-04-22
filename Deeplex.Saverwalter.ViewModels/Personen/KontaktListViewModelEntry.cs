using Deeplex.Saverwalter.Model;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModelEntry
    {
        public override string ToString()
            => Entity.Bezeichnung;

        public Type Type { get; }
        public int Id { get; }
        public Guid Guid { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }
        public IPerson Entity { get; }

        public KontaktListViewModelEntry(Guid id, AppViewModel avm) : this(avm.ctx.FindPerson(id)) { }
        public KontaktListViewModelEntry(JuristischePerson j) : this(j as IPerson)
        {
            Type = j.GetType();
            Id = j.JuristischePersonId;
            Vorname = "";
            Name = j.Bezeichnung;
        }

        public KontaktListViewModelEntry(NatuerlichePerson k) : this(k as IPerson)
        {
            Type = k.GetType();
            Id = k.NatuerlichePersonId;
            Vorname = k.Vorname ?? "";
            Name = k.Nachname;
        }

        private KontaktListViewModelEntry(IPerson p)
        {
            Entity = p;
            Guid = p.PersonId;
            Email = p.Email ?? "";
            Telefon = p.Telefon ?? "";
            Mobil = p.Mobil ?? "";
            Anschrift = p.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
