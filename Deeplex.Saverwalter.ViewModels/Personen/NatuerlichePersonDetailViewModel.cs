using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class NatuerlichePersonDetailViewModel : IPersonDetailViewModel, IDetailViewModel
    {
        public new NatuerlichePerson Entity => (NatuerlichePerson)base.Entity;

        public int Id { get; private set; }

        public List<Anrede> Anreden { get; }

        public SavableProperty<Anrede> Anrede { get; private set; }
        public SavableProperty<string> Vorname { get; private set; }
        public SavableProperty<string> Nachname { get; private set; }

        public bool WohnungenInklusiveJurPers
        {
            get => mInklusiveZusatz;
            set
            {
                mInklusiveZusatz = value;
                UpdateListen();
            }
        }

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> JuristischePersonen = new();

        public void UpdateListen()
        {
            JuristischePersonen.Value = Entity.JuristischePersonen
                .Select(w => new KontaktListViewModelEntry(WalterDbService, w.PersonId))
                .ToImmutableList();

            Wohnungen.Value = WalterDbService.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveJurPers && JuristischePersonen.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, WalterDbService))
                .ToImmutableList();
        }

        public void SetEntity(NatuerlichePerson k)
        {
            base.SetEntity(k);
            Id = k.NatuerlichePersonId;
            Anrede = new(this, k.Anrede);
            Vorname = new(this, k.Vorname);
            Nachname = new(this, k.Nachname);

            UpdateListen();
        }

        public NatuerlichePersonDetailViewModel(INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            Anreden = Enums.Anreden;

            Save = new RelayCommand(_ =>
            {
                save();
                Entity.Nachname = Nachname.Value;
                Entity.Vorname = Vorname.Value;
                Entity.Anrede = Anrede.Value;

                if (Entity.NatuerlichePersonId != 0)
                {
                    WalterDbService.ctx.NatuerlichePersonen.Update(Entity);
                }
                else
                {
                    WalterDbService.ctx.NatuerlichePersonen.Add(Entity);
                }
                WalterDbService.SaveWalter();
                checkForChanges();
            }, _ => true);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    base.WalterDbService.ctx.NatuerlichePersonen.Remove(Entity);
                    base.WalterDbService.SaveWalter();
                }
            }, _ => true);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                BaseCheckForChanges() ||
                    Entity.Anrede != Anrede.Value ||
                    Entity.Vorname != Vorname.Value ||
                    Entity.Nachname != Nachname.Value;
        }
    }
}
