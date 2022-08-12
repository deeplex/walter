using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungDetailViewModel : DetailViewModel<Wohnung>, IDetailViewModel
    {
        public Wohnung Entity { get; private set; }
        public int Id => Entity.WohnungId;

        public override string ToString() => AdresseViewModel.Anschrift(Entity) + " - " + Bezeichnung.Value;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new(DateTime.Now.Year - 1);
        public ObservableProperty<bool> ZeigeVorlagen = new();

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;

        public SavableProperty<Adresse> Adresse { get; set; }
        public string Anschrift => AdresseViewModel.Anschrift(Adresse.Value.AdresseId, WalterDbService);

        public KontaktListViewModelEntry Besitzer { get; set; }

        public SavableProperty<string> Bezeichnung { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<double> Wohnflaeche { get; set; }
        public SavableProperty<double> Nutzflaeche { get; set; }
        public SavableProperty<int> Nutzeinheit { get; set; }

        public RelayCommand RemoveBesitzer;

        public WohnungDetailViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;

            AlleVermieter = WalterDbService.ctx.JuristischePersonen.ToImmutableList()
                .Where(j => j.isVermieter == true).Select(j => new KontaktListViewModelEntry(j))
                .Concat(WalterDbService.ctx.NatuerlichePersonen
                    .Where(n => n.isVermieter == true).Select(n => new KontaktListViewModelEntry(n)))
                .ToImmutableList();

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    WalterDbService.ctx.Wohnungen.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            }, _ => true);

            Save = new RelayCommand(_ => save(), _ => true);
            RemoveBesitzer = new RelayCommand(_ => { Besitzer = null; }, _ => true);
        }

        public override void SetEntity(Wohnung e)
        {
            Entity = e;

            if (e.BesitzerId != Guid.Empty)
            {
                Besitzer = AlleVermieter.SingleOrDefault(k => k.Entity.PersonId == e.BesitzerId);
            }
            Bezeichnung = new(this, e.Bezeichnung);
            Notiz = new(this, e.Notiz);
            Wohnflaeche = new(this, e.Wohnflaeche);
            Nutzflaeche = new(this, e.Nutzflaeche);
            Nutzeinheit = new(this, e.Nutzeinheit);
        }

        private void save()
        {
            Entity.Adresse = Adresse.Value;
            Entity.Bezeichnung = Bezeichnung.Value;
            Entity.Wohnflaeche = Wohnflaeche.Value;
            Entity.Nutzflaeche = Nutzflaeche.Value;
            Entity.Nutzeinheit = Nutzeinheit.Value;
            Entity.Notiz = Notiz.Value;

            if (Entity.WohnungId != 0)
            {
                WalterDbService.ctx.Wohnungen.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Wohnungen.Add(Entity);
            }
            WalterDbService.SaveWalter();
            checkForChanges();
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Adresse.AdresseId != Adresse.Value.AdresseId ||
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Wohnflaeche != Wohnflaeche.Value ||
                Entity.Nutzflaeche != Nutzflaeche.Value ||
                Entity.Nutzeinheit != Nutzeinheit.Value ||
                Entity.Notiz != Notiz.Value;
        }
    }
}
