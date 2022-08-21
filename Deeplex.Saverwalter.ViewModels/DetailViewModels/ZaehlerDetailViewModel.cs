using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerDetailViewModel : DetailViewModel<Zaehler>, IDetailViewModel
    {
        public override string ToString() => Kennnummer.Value;

        public ObservableProperty<ZaehlerstandListViewModel> Staende { get; private set; } = new();

        public List<Zaehlertyp> Typen => Enums.Zaehlertypen;
        public List<WohnungListViewModelEntry> Wohnungen = new List<WohnungListViewModelEntry>();

        public List<ZaehlerListViewModelEntry> AllgemeinzaehlerList { get; private set; }
        public SavableProperty<ZaehlerListViewModelEntry> Allgemeinzaehler { get; set; }
        public SavableProperty<Zaehlertyp> Typ { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<string> Kennnummer { get; set; }
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; set; }

        public override void SetEntity(Zaehler z)
        {
            Entity = z;
            Typ = new(this, z.Typ);
            Notiz = new(this, z.Notiz);
            Kennnummer = new(this, z.Kennnummer);
            Wohnung = new(this, Wohnungen.FirstOrDefault(e => z.Wohnung == e.Entity));

            AllgemeinzaehlerList = WalterDbService.ctx.ZaehlerSet
               .Where(y => y.ZaehlerId != Id && y.Wohnung == null)
                   .Select(y => new ZaehlerListViewModelEntry(y))
                   .ToList();
            Allgemeinzaehler = new(this, AllgemeinzaehlerList.FirstOrDefault(e => z.Allgemeinzaehler == e.Entity));

            Staende.Value = new ZaehlerstandListViewModel(z, NotificationService, WalterDbService);
        }

        public ZaehlerDetailViewModel(INotificationService ns, IWalterDbService db): base(ns, db)
        {
            Wohnungen = WalterDbService.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListViewModelEntry(w, WalterDbService))
                .ToList();

            DeleteAllgemeinzaehler = new RelayCommand(_ => Allgemeinzaehler.Value = null);
            DeleteWohnung = new RelayCommand(_ => Wohnung.Value = null);

            Save = new RelayCommand(_ =>
            {
                Entity.Kennnummer = Kennnummer.Value;
                Entity.Wohnung = Wohnung.Value?.Entity;
                Entity.Typ = Typ.Value;
                Entity.Allgemeinzaehler = Allgemeinzaehler.Value?.Entity;
                Entity.Notiz = Notiz.Value;
                save();
                Staende.Value.Liste.Value?.ForEach(e => e.Save.Execute(null));
            }, _ => true);
        }

        public RelayCommand DeleteAllgemeinzaehler { get; }
        public RelayCommand DeleteWohnung { get; }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Kennnummer.Value != Entity.Kennnummer ||
                Wohnung.Value?.Id != Entity.Wohnung?.WohnungId ||
                Typ.Value != Entity.Typ ||
                Notiz.Value != Entity.Notiz ||
                Allgemeinzaehler.Value?.Id != Entity.Allgemeinzaehler?.ZaehlerId;
        }
    }
}
