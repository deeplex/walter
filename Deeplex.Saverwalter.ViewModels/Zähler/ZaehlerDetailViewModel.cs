using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerDetailViewModel : BindableBase, IDetailViewModel
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

        public override string ToString() => Kennnummer.Value;

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

        public SavableProperty<Zaehlertyp> Typ { get; }
        public SavableProperty<string> Notiz { get; }
        public SavableProperty<string> Kennnummer { get; }
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; }

        // Necessary to show / hide Zählerstände
        public bool Initialized => Entity.ZaehlerId != 0;

        private INotificationService NotificationService;
        private IWalterDbService Db;

        public ZaehlerDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Zaehler(), ns, db) { }
        public ZaehlerDetailViewModel(Zaehler z, INotificationService ns, IWalterDbService db)
        {
            Typ = new(this, z.Typ);
            Notiz = new(this, z.Notiz);
            Kennnummer = new(this, z.Kennnummer);
            if (z.Wohnung != null)
            {
                Wohnung = new(this, new WohnungListViewModelEntry(z.Wohnung, db));
            }
            else
            {
                Wohnung = new(this);
            }

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

            Staende.Value = new ZaehlerstandListViewModel(z, NotificationService, Db);

            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung.Value = null);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Entity.Staende.ForEach(s => Db.ctx.Zaehlerstaende.Remove(s));
                    Db.ctx.ZaehlerSet.Remove(Entity);
                    Db.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;
        public AsyncRelayCommand Delete { get; }
        public RelayCommand Save { get; }

        public void checkForChanges()
        {
            NotificationService.outOfSync =
                Kennnummer.Value != Entity.Kennnummer ||
                Wohnung.Value?.Id != Entity.Wohnung?.WohnungId ||
                Typ.Value != Entity.Typ ||
                Notiz.Value != Entity.Notiz ||
                AllgemeinZaehler?.Id != Entity.AllgemeinZaehler?.ZaehlerId;
        }

        private void save()
        {
            Entity.Kennnummer = Kennnummer.Value;
            Entity.Wohnung = Wohnung.Value?.Entity;
            Entity.Typ = Typ.Value;
            Entity.AllgemeinZaehler = AllgemeinZaehler?.Entity;
            Entity.Notiz = Notiz.Value;

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
            Staende.Value.Liste.Value.ForEach(e => e.Save.Execute(null));
            checkForChanges();
        }
    }
}
