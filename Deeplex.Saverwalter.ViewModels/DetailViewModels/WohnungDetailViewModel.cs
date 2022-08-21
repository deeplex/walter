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
        public override string ToString() => AdresseViewModel.Anschrift(Entity) + " - " + Bezeichnung.Value;

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;

        public SavableProperty<KontaktListViewModelEntry> Besitzer { get; set; }
        public SavableProperty<AdresseViewModel<Wohnung>> Adresse { get; set; }
        public SavableProperty<string> Bezeichnung { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<double> Wohnflaeche { get; set; }
        public SavableProperty<double> Nutzflaeche { get; set; }
        public SavableProperty<int> Nutzeinheit { get; set; }

        public RelayCommand RemoveBesitzer;

        public WohnungDetailViewModel(INotificationService ns, IWalterDbService db) : base(ns, db)
        {
            AlleVermieter = WalterDbService.ctx.JuristischePersonen.ToImmutableList()
                .Select(j => new KontaktListViewModelEntry(j))
                .Concat(WalterDbService.ctx.NatuerlichePersonen
                    .Select(n => new KontaktListViewModelEntry(n)))
                .ToImmutableList();

            Save = new RelayCommand(_ =>
            {
                Adresse.Value.save();
                Entity.BesitzerId = Besitzer.Value.Entity.PersonId;
                Entity.Adresse = Adresse.Value.Entity;
                Entity.Bezeichnung = Bezeichnung.Value;
                Entity.Wohnflaeche = Wohnflaeche.Value;
                Entity.Nutzflaeche = Nutzflaeche.Value;
                Entity.Nutzeinheit = Nutzeinheit.Value;
                Entity.Notiz = Notiz.Value;
                save();
            }, _ => true);
            RemoveBesitzer = new RelayCommand(_ => { Besitzer = null; }, _ => true);
        }

        public override void SetEntity(Wohnung e)
        {
            Entity = e;

            Besitzer = new(this, AlleVermieter.SingleOrDefault(k => k.Entity.PersonId == e.BesitzerId));
            Bezeichnung = new(this, e.Bezeichnung);
            Notiz = new(this, e.Notiz);
            Wohnflaeche = new(this, e.Wohnflaeche);
            Nutzflaeche = new(this, e.Nutzflaeche);
            Nutzeinheit = new(this, e.Nutzeinheit);
            Adresse = new(this, new(e, WalterDbService, NotificationService));
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Adresse.AdresseId != Adresse.Value.Id ||
                Entity.BesitzerId != Besitzer.Value.Entity.PersonId ||
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Wohnflaeche != Wohnflaeche.Value ||
                Entity.Nutzflaeche != Nutzflaeche.Value ||
                Entity.Nutzeinheit != Nutzeinheit.Value ||
                Entity.Notiz != Notiz.Value;
        }
    }
}
