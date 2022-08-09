using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class JuristischePersonViewModel : PersonViewModel, IDetailViewModel
    {
        public new JuristischePerson Entity => (JuristischePerson)base.Entity;
        public int Id;

        public SavableProperty<string> Bezeichnung { get; private set; }
        public override string ToString() => Bezeichnung.Value;

        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Mitglieder = new();
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> AddMitglieder = new();
        public ObservableProperty<KontaktListViewModelEntry> AddMitglied = new();

        public bool WohnungenInklusiveMitglieder
        {
            get => mInklusiveZusatz;
            set
            {
                mInklusiveZusatz = value;
                UpdateListen();
            }
        }

        public void UpdateListen()
        {
            Mitglieder.Value = Entity.Mitglieder
                .Select(w => new KontaktListViewModelEntry(WalterDbService, w.PersonId))
                .ToImmutableList();

            AddMitglieder.Value = WalterDbService.ctx.NatuerlichePersonen
                .Select(k => new KontaktListViewModelEntry(k))
                .ToList()
                .Concat(WalterDbService.ctx.JuristischePersonen
                    .Select(k => new KontaktListViewModelEntry(k))
                    .ToList())
                .Where(k => !Mitglieder.Value.Any(e => e.Entity.PersonId == k.Entity.PersonId))
                    .ToImmutableList();

            Wohnungen.Value = WalterDbService.ctx.Wohnungen
                .ToList()
                .Where(w => w.BesitzerId == Entity.PersonId ||
                    (WohnungenInklusiveMitglieder && Mitglieder.Value.Any(m => m.Entity.PersonId == w.BesitzerId)))
                .Select(w => new WohnungListViewModelEntry(w, WalterDbService))
                .ToImmutableList();
        }

        public RelayCommand AddMitgliedCommand;

        public void SetEntity(JuristischePerson k)
        {
            base.SetEntity(k);
            Id = k.JuristischePersonId;
            Bezeichnung = new(this, k.Bezeichnung);

            UpdateListen();
        }

        public JuristischePersonViewModel(INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            AddMitgliedCommand = new RelayCommand(_ =>
            {
                if (AddMitglied.Value?.Entity.PersonId is Guid guid)
                {
                    if (AddMitglied.Value.Entity is NatuerlichePerson n)
                    {
                        n.JuristischePersonen.Add(Entity);
                    }
                    else if (AddMitglied.Value.Entity is JuristischePerson j)
                    {
                        j.JuristischePersonen.Add(Entity);
                    }
                    WalterDbService.SaveWalter();
                    UpdateListen();
                }
            }, _ => true);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    WalterDbService.ctx.JuristischePersonen.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ =>
            {
                save();
                Entity.Bezeichnung = Bezeichnung.Value;

                if (Entity.JuristischePersonId != 0)
                {
                    WalterDbService.ctx.JuristischePersonen.Update(Entity);
                }
                else
                {
                    WalterDbService.ctx.JuristischePersonen.Add(Entity);
                }
                WalterDbService.SaveWalter();
            }, _ => true);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                BaseCheckForChanges() ||
                Entity.Bezeichnung != Bezeichnung.Value;
        }
    }
}
