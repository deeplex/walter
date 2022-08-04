using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModelEntry
    {
        public override string ToString() => Entity.Bezeichnung;

        public Type Type { get; }
        public Guid Guid => Entity.PersonId;
        public string Anschrift => AdresseViewModel.Anschrift(Entity);
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }
        public IPerson Entity { get; }

        public KontaktListViewModelEntry(IWalterDbService db, Guid personId) : this(db.ctx.FindPerson(personId)) { }
        public KontaktListViewModelEntry(JuristischePerson j) : this(j as IPerson)
        {
            Entity = j;
            Type = j.GetType();
        }

        public KontaktListViewModelEntry(NatuerlichePerson k) : this(k as IPerson)
        {
            Entity = k;
            Type = k.GetType();
        }

        private KontaktListViewModelEntry(IPerson p)
        {
            Entity = p;
        }
    }
}
