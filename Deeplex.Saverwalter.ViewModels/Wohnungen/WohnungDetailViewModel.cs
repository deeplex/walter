using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase
    {
        public Wohnung Entity { get; }
        public int Id => Entity.WohnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new(DateTime.Now.Year - 1);
        public ObservableProperty<bool> ZeigeVorlagen = new();

        public async Task selfDestruct()
        {
            if (await NotificationService.Confirmation())
            {
                Db.ctx.Wohnungen.Remove(Entity);
                Db.SaveWalter();
            }
        }

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId, Db);

        public KontaktListViewModelEntry Besitzer { get; set; }

        public ObservableProperty<string> Bezeichnung { get; set; } = new();
        public ObservableProperty<string> Notiz { get; set; } = new();
        public ObservableProperty<double> Wohnflaeche { get; set; } = new();
        public ObservableProperty<double> Nutzflaeche { get; set; } = new();
        public ObservableProperty<int> Nutzeinheit { get; set; } = new();

        private INotificationService NotificationService;
        private IWalterDbService Db;
        public RelayCommand RemoveBesitzer;
        public RelayCommand Save;

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
            Bezeichnung.Value = w.Bezeichnung;
            Notiz.Value = w.Notiz;
            Wohnflaeche.Value = w.Wohnflaeche;
            Nutzflaeche.Value = w.Nutzflaeche;
            Nutzeinheit.Value = w.Nutzeinheit;

            Save = new RelayCommand(_ => save(), _ => true); // Should be NotificationService.outOfSync
            RemoveBesitzer = new RelayCommand(_ => { Besitzer = null; }, _ => true);
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
