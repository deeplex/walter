using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerDetailViewModel : DetailViewModel<Zaehler>, DetailViewModel
    {
        public Zaehler Entity { get; private set; }
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

        public SavableProperty<Zaehlertyp> Typ { get; private set; }
        public SavableProperty<string> Notiz { get; private set; }
        public SavableProperty<string> Kennnummer { get; private set; }
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; private set; }

        // Necessary to show / hide Zählerstände
        public bool Initialized => Entity.ZaehlerId != 0;

        public override void SetEntity(Zaehler z)
        {
            Entity = z;
            mId = Entity.ZaehlerId;
            Typ = new(this, z.Typ);
            Notiz = new(this, z.Notiz);
            Kennnummer = new(this, z.Kennnummer);

            if (z.Wohnung != null)
            {
                Wohnung = new(this, new WohnungListViewModelEntry(z.Wohnung, WalterDbService));
            }
            else
            {
                Wohnung = new(this);
            }

            EinzelZaehler = WalterDbService.ctx.ZaehlerSet
               .Where(y => y.ZaehlerId != Id)
                   .Select(y => new ZaehlerListViewModelEntry(y))
                   .ToList();
            Staende.Value = new ZaehlerstandListViewModel(z, NotificationService, WalterDbService);
            AllgemeinZaehler = mAllgemeinZaehler = EinzelZaehler.SingleOrDefault(y => y.Id == z.AllgemeinZaehler?.ZaehlerId);
        }

        public ZaehlerDetailViewModel(INotificationService ns, IWalterDbService db)
        {

            NotificationService = ns;
            WalterDbService = db;

            Wohnungen = WalterDbService.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, WalterDbService))
                .ToList();

            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung.Value = null);

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotificationService.Confirmation())
                {
                    Entity.Staende.ForEach(s => WalterDbService.ctx.Zaehlerstaende.Remove(s));
                    WalterDbService.ctx.ZaehlerSet.Remove(Entity);
                    WalterDbService.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
        }

        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;

        public override void checkForChanges()
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
                WalterDbService.ctx.ZaehlerSet.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.ZaehlerSet.Add(Entity);
            }
            WalterDbService.SaveWalter();
            if (mId != Entity.ZaehlerId)
            {
                Id = Entity.ZaehlerId;
                Staende.Value = new ZaehlerstandListViewModel(Entity, NotificationService, WalterDbService);
            }
            Staende.Value.Liste.Value.ForEach(e => e.Save.Execute(null));
            checkForChanges();
        }
    }
}
