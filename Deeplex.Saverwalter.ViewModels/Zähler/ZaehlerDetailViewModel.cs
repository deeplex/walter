using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerDetailViewModel : BindableBase
    {
        public Zaehler Entity;
        private int mId { get; set; }
        public int Id
        {
            get => mId;
            set
            {
                var old = mId;
                Entity.ZaehlerId = value;
                mId = value;
                RaisePropertyChangedAuto(old, value);
                RaisePropertyChanged(nameof(Initialized));
            }
        }

        public ObservableProperty<ZaehlerstandListViewModel> Staende = new();

        public List<Zaehlertyp> Typen => Enums.Zaehlertypen;
        public List<WohnungListViewModelEntry> Wohnungen = new List<WohnungListViewModelEntry>();

        public List<ZaehlerListViewModelEntry> EinzelZaehler;

        private ZaehlerListViewModelEntry mAllgemeinZaehler;
        public ZaehlerListViewModelEntry AllgemeinZaehler
        {
            get => mAllgemeinZaehler;
            set
            {
                var old = Entity.AllgemeinZaehler?.ZaehlerId;
                mAllgemeinZaehler = EinzelZaehler.SingleOrDefault(z => z.Id == value?.Id);
                Entity.AllgemeinZaehler = mAllgemeinZaehler?.Entity;
                RaisePropertyChangedAuto(old, mAllgemeinZaehler?.Id);
            }
        }

        public Zaehlertyp Typ
        {
            get => Entity.Typ;
            set
            {
                var old = Entity.Typ;
                Entity.Typ = value;
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

        public string Kennnummer
        {
            get => Entity.Kennnummer;
            set
            {
                var old = Entity.Kennnummer;
                Entity.Kennnummer = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        private WohnungListViewModelEntry mWohnung;
        public WohnungListViewModelEntry Wohnung
        {
            get => mWohnung;
            set
            {
                var old = Entity.Wohnung;
                Entity.Wohnung = value?.Entity;
                mWohnung = value;
                RaisePropertyChangedAuto(old, value?.Entity);
            }
        }

        // Necessary to show / hide Zählerstände
        public bool Initialized => Entity.ZaehlerId != 0;

        private INotificationService NotificationService;
        private IWalterDbService Db;

        public ZaehlerDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Zaehler(), ns, db) { }
        public ZaehlerDetailViewModel(Zaehler z, INotificationService ns, IWalterDbService db)
        {
            NotificationService = ns;
            Db = db;
            Entity = z;
            mId = Entity.ZaehlerId;

            Wohnungen = Db.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, Db))
                .ToList();

            EinzelZaehler = Db.ctx.ZaehlerSet
               .Where(y => y.ZaehlerId != Id)
               .Select(y => new ZaehlerListViewModelEntry(y))
               .ToList();
            AllgemeinZaehler = mAllgemeinZaehler = EinzelZaehler.SingleOrDefault(y => y.Id == z.AllgemeinZaehler?.ZaehlerId);


            if (mId != 0)
            {
                Staende.Value = new ZaehlerstandListViewModel(z, NotificationService, Db);
                Wohnung = Wohnungen.Find(w => w.Id == z.WohnungId);
            }

            PropertyChanged += OnUpdate;

            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung = null);
        }

        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;

        public async Task SelfDestruct()
        {
            if (await NotificationService.Confirmation())
            {
                Entity.Staende.ForEach(s => Db.ctx.Zaehlerstaende.Remove(s));
                Db.ctx.ZaehlerSet.Remove(Entity);
                Db.SaveWalter();
            }
        }

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Kennnummer):
                case nameof(Wohnung):
                case nameof(Typ):
                case nameof(AllgemeinZaehler):
                    break;
                default:
                    return;
            }

            if (Entity.Kennnummer == "" || Entity.Kennnummer == null)
            {
                return;
            }

            if (Entity.ZaehlerId != 0)
            {
                Db.ctx.ZaehlerSet.Update(Entity);
            }
            else
            {
                Db.ctx.ZaehlerSet.Add(Entity);
            }
            Db.SaveWalter();
            if (mId != Entity.ZaehlerId)
            {
                Id = Entity.ZaehlerId;
                Staende.Value = new ZaehlerstandListViewModel(Entity, NotificationService, Db);
            }
        }
    }
}
