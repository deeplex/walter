using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungDetailViewModel : ValidatableBase
    {
        public Wohnung Entity;
        public int Id => Entity.WohnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new ObservableProperty<int>(DateTime.Now.Year - 1);
        public ObservableProperty<bool> ZeigeVorlagen = new ObservableProperty<bool>();

        public async Task selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Db.ctx.Wohnungen.Remove(Entity);
                Db.SaveWalter();
            }
        }

        public ImmutableList<KontaktListViewModelEntry> AlleVermieter;

        private KontaktListViewModelEntry mBesitzer;
        public KontaktListViewModelEntry Besitzer
        {
            get => mBesitzer;
            set
            {
                Entity.BesitzerId = value == null ? Guid.Empty : value.Entity.PersonId;
                mBesitzer = value;
                RaisePropertyChangedAuto();
            }
        }

        public int AdresseId => Entity.AdresseId;
        public string Anschrift => AdresseViewModel.Anschrift(AdresseId, Db);

        public string Bezeichnung
        {
            get => Entity.Bezeichnung;
            set
            {
                var old = Entity.Bezeichnung;
                Entity.Bezeichnung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string Notiz
        {
            get => Entity.Notiz;
            set
            {
                var old = Entity.Notiz;
                Entity.Notiz = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double Wohnflaeche
        {
            get => Entity.Wohnflaeche;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
                var old = Entity.Wohnflaeche;
                Entity.Wohnflaeche = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public double Nutzflaeche
        {
            get => Entity.Nutzflaeche;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
                var old = Entity.Nutzflaeche;
                Entity.Nutzflaeche = val;
                RaisePropertyChangedAuto(old, val);
            }
        }

        public int Nutzeinheit
        {
            get => Entity.Nutzeinheit;
            set
            {
                var val = int.MinValue == value ? 0 : value;
                var old = Entity.Nutzeinheit;
                Entity.Nutzeinheit = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        private IAppImplementation Impl;
        private IWalterDbService Db;
        public RelayCommand RemoveBesitzer;

        public WohnungDetailViewModel(IAppImplementation impl, IWalterDbService db) : this(new Wohnung(), impl, db) { }
        public WohnungDetailViewModel(Wohnung w, IAppImplementation impl, IWalterDbService db)
        {
            Entity = w;
            Db = db;
            Impl = impl;

            AlleVermieter = Db.ctx.JuristischePersonen.ToImmutableList()
                .Where(j => j.isVermieter == true).Select(j => new KontaktListViewModelEntry(j))
                .Concat(Db.ctx.NatuerlichePersonen
                    .Where(n => n.isVermieter == true).Select(n => new KontaktListViewModelEntry(n)))
                .ToImmutableList();

            if (w.BesitzerId != Guid.Empty)
            {
                Besitzer = AlleVermieter.SingleOrDefault(e => e.Entity.PersonId == w.BesitzerId);
            }

            PropertyChanged += OnUpdate;

            RemoveBesitzer = new RelayCommand(_ => { Besitzer = null; }, _ => true);
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Besitzer):
                case nameof(Bezeichnung):
                case nameof(Wohnflaeche):
                case nameof(Nutzflaeche):
                case nameof(Nutzeinheit):
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            if ((Entity.Adresse == null && Entity.AdresseId == 0) ||
                Entity.Bezeichnung == null || Entity.Bezeichnung == "")
            {
                return;
            }

            if (Entity.WohnungId != 0)
            {
                Db.ctx.Wohnungen.Update(Entity);
            }
            else
            {
                Db.ctx.Wohnungen.Add(Entity);
            }
            Db.SaveWalter();
        }
    }
}
