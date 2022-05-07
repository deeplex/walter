using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModelEntry
    {
        public override string ToString()
            => Entity.Bezeichnung;

        public Type Type { get; }
        public Guid Guid { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift => AdresseViewModel.Anschrift(Entity);
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }
        public IPerson Entity { get; }

        public KontaktListViewModelEntry(Guid id, IWalterDbService avm) : this(avm.ctx.FindPerson(id)) { }
        public KontaktListViewModelEntry(JuristischePerson j) : this(j as IPerson)
        {
            Entity = j;
            Type = j.GetType();
            Name = j.Bezeichnung;
        }

        public KontaktListViewModelEntry(NatuerlichePerson k) : this(k as IPerson)
        {
            Entity = k;
            Type = k.GetType();
            Vorname = k.Vorname ?? "";
            Name = k.Nachname;
        }

        private KontaktListViewModelEntry(IPerson p)
        {
            Entity = p;
            Email = p.Email ?? "";
            Telefon = p.Telefon ?? "";
            Mobil = p.Mobil ?? "";
        }
    }
}
