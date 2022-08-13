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
        public int Id => Entity.WohnungId;

        public override string ToString() => AdresseViewModel.Anschrift(Entity) + " - " + Bezeichnung.Value;

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;
        public KontaktListViewModelEntry Besitzer { get; set; }

        public SavableProperty<AdresseViewModel<Wohnung>> Adresse { get; set; }
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
            Adresse = new(this, new(e, WalterDbService, NotificationService));
        }

        private void save()
        {
            Adresse.Value.save();
            Entity.Adresse = Adresse.Value.Entity;
            Entity.Bezeichnung = Bezeichnung.Value;
            Entity.Wohnflaeche = Wohnflaeche.Value;
            Entity.Nutzflaeche = Nutzflaeche.Value;
            Entity.Nutzeinheit = Nutzeinheit.Value;
            Entity.Notiz = Notiz.Value;
            save();
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Adresse.AdresseId != Adresse.Value.Id ||
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Wohnflaeche != Wohnflaeche.Value ||
                Entity.Nutzflaeche != Nutzflaeche.Value ||
                Entity.Nutzeinheit != Nutzeinheit.Value ||
                Entity.Notiz != Notiz.Value;
        }
    }
}
