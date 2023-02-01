using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ZaehlerDetailViewModel : DetailViewModel<Zaehler>, IDetailViewModel
    {
        public override string ToString() => Kennnummer.Value;

        public ObservableProperty<ZaehlerstandListViewModel> Staende { get; private set; } = new();

        public List<Zaehlertyp> Typen => Enums.Zaehlertypen;
        public List<AdresseViewModel> Adressen = new();
        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

        public List<ZaehlerListViewModelEntry> AllgemeinzaehlerList { get; private set; }
        public SavableProperty<ZaehlerListViewModelEntry> Allgemeinzaehler { get; set; }
        public SavableProperty<Zaehlertyp> Typ { get; set; }
        public SavableProperty<string> Notiz { get; set; }
        public SavableProperty<string> Kennnummer { get; set; }
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; set; }
        private AdresseViewModel mAdresse { get; set; }
        public AdresseViewModel Adresse
        {
            get => mAdresse;
            set
            {
                if (value == mAdresse)
                {
                    return;
                }

                mAdresse = value;
                Wohnungen.Value = mAdresse?.Entity.Wohnungen.Select(e => new WohnungListViewModelEntry(e, WalterDbService)).ToImmutableList();
                if (Wohnung != null)
                {
                    Wohnung.Value = null;
                }
                RaisePropertyChanged(nameof(Wohnungen));
                RaisePropertyChanged(nameof(Wohnung));
                RaisePropertyChangedAuto();
            }
        }

        public override void SetEntity(Zaehler z)
        {
            Entity = z;
            Typ = new(this, z.Typ);
            Notiz = new(this, z.Notiz);
            Kennnummer = new(this, z.Kennnummer);
            Adresse = Adressen.FirstOrDefault(e => z.Adresse == e.Entity);

            Wohnung = new(this, Wohnungen.Value.FirstOrDefault(e => z.Wohnung == e.Entity));

            AllgemeinzaehlerList = WalterDbService.ctx.ZaehlerSet
               .Where(y => y.ZaehlerId != Id && y.Wohnung == null)
                   .Select(y => new ZaehlerListViewModelEntry(y))
                   .ToList();
            Allgemeinzaehler = new(this, AllgemeinzaehlerList.FirstOrDefault(e => z.Allgemeinzaehler == e.Entity));

            Staende.Value = new ZaehlerstandListViewModel(z, NotificationService, WalterDbService);
        }

        public ZaehlerDetailViewModel(INotificationService ns, IWalterDbService db): base(ns, db)
        {
            Adressen = WalterDbService.ctx.Adressen
                .Include(a => a.Wohnungen)
                .ToList()
                .Select(a => new AdresseViewModel(a, WalterDbService, NotificationService))
                .ToList();
            
            DeleteAllgemeinzaehler = new RelayCommand(_ => Allgemeinzaehler.Value = null);
            DeleteAdresse = new RelayCommand(_ => Adresse = null);

            Save = new RelayCommand(_ =>
            {
                Entity.Kennnummer = Kennnummer.Value;
                Entity.Adresse = Adresse.Entity;
                Entity.Wohnung = Wohnung.Value?.Entity;
                Entity.Typ = Typ.Value;
                Entity.Allgemeinzaehler = Allgemeinzaehler.Value?.Entity;
                Entity.Notiz = Notiz.Value;
                save();
                Staende.Value.Liste.Value?.ForEach(e => e.Save.Execute(null));
            }, _ => true);
        }

        public RelayCommand DeleteAllgemeinzaehler { get; }
        public RelayCommand DeleteAdresse { get; }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Kennnummer.Value != Entity.Kennnummer ||
                Wohnung.Value?.Id != Entity.Wohnung?.WohnungId ||
                Adresse?.Id != Entity.Adresse?.AdresseId ||
                Typ.Value != Entity.Typ ||
                Notiz.Value != Entity.Notiz ||
                Allgemeinzaehler.Value?.Id != Entity.Allgemeinzaehler?.ZaehlerId;
        }
    }
}
