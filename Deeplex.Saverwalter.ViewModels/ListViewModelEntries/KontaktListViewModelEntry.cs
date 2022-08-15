using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModelEntry : ListViewModelEntry<IPerson>, IDeletableEntry
    {
        public override string ToString() => Entity.Bezeichnung;

        public Type Type { get; }
        public Guid Guid => Entity.PersonId;
        public string Anschrift => AdresseViewModel.Anschrift(Entity);
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }
        public IPerson Entity { get; }

        public AsyncRelayCommand Delete { get; }

        public KontaktListViewModelEntry(IMemberListViewModel parent, IPerson p) : this(p)
        {
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (Entity.PersonId != Guid.Empty && await parent.NotificationService.Confirmation())
                {
                    parent.WalterDbService.ctx.Remove(Entity);
                    parent.WalterDbService.SaveWalter();
                }
                parent.List.Value = parent.List.Value.Remove(this);
            }, _ => true);
        }
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
            Email = p.Email;
            Telefon = p.Telefon;
            Mobil = p.Mobil;
        }
    }
}
