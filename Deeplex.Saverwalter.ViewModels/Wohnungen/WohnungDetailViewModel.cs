using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase, IDetail
    {
        public Wohnung Entity { get; }
        public int Id => Entity.WohnungId;

        public override string ToString() => Bezeichnung.Value;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new(DateTime.Now.Year - 1);
        public ObservableProperty<bool> ZeigeVorlagen = new();

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId, Db);

        public KontaktListViewModelEntry Besitzer { get; set; }

        public SavableProperty<string> Bezeichnung { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<double> Wohnflaeche { get; set; }
        public SavableProperty<double> Nutzflaeche { get; set; }
        public SavableProperty<int> Nutzeinheit { get; set; }

        private INotificationService NotificationService;
        private IWalterDbService Db;
        public RelayCommand RemoveBesitzer;
        public RelayCommand Save { get; }
        public AsyncRelayCommand Delete { get; }

        public WohnungDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Wohnung(), ns, db) { }
        public WohnungDetailViewModel(Wohnung w, INotificationService ns, IWalterDbService db)
        {
            Entity = w;
            Db = db;
            NotificationService = ns;

            AlleVermieter = Db.ctx.JuristischePersonen.ToImmutableList()
                .Where(j => j.isVermieter == true).Select(j => new KontaktListViewModelEntry(j))
                .Concat(Db.ctx.NatuerlichePersonen
                    .Where(n => n.isVermieter == true).Select(n => new KontaktListViewModelEntry(n)))
                .ToImmutableList();

            if (w.BesitzerId != Guid.Empty)
            {
                Besitzer = AlleVermieter.SingleOrDefault(e => e.Entity.PersonId == w.BesitzerId);
            }
            Bezeichnung = new(this, w.Bezeichnung);
            Notiz = new(this, w.Notiz);
            Wohnflaeche = new(this, w.Wohnflaeche);
            Nutzflaeche = new(this, w.Nutzflaeche);
            Nutzeinheit = new(this, w.Nutzeinheit);

            Save = new RelayCommand(_ => save(), _ => true); // Should be NotificationService.outOfSync
            RemoveBesitzer = new RelayCommand(_ => { Besitzer = null; }, _ => true);
            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Db.ctx.Wohnungen.Remove(Entity);
                    Db.SaveWalter();
                }
            }, _ => true);
        }

        private void save()
        {
            Entity.Bezeichnung = Bezeichnung.Value;
            Entity.Wohnflaeche = Wohnflaeche.Value;
            Entity.Nutzflaeche = Nutzflaeche.Value;
            Entity.Nutzeinheit = Nutzeinheit.Value;
            Entity.Notiz = Notiz.Value;

            if (Entity.WohnungId != 0)
            {
                Db.ctx.Wohnungen.Update(Entity);
            }
            else
            {
                Db.ctx.Wohnungen.Add(Entity);
            }
            Db.SaveWalter();
            checkForChanges();
        }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Entity.Bezeichnung != Bezeichnung.Value ||
                Entity.Wohnflaeche != Wohnflaeche.Value ||
                Entity.Nutzflaeche != Nutzflaeche.Value ||
                Entity.Nutzeinheit != Nutzeinheit.Value ||
                Entity.Notiz != Notiz.Value;
        }
    }
}
