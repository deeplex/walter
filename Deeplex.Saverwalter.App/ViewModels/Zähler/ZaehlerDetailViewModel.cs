using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        public ObservableProperty<ZaehlerstandListViewModel> Staende
            = new ObservableProperty<ZaehlerstandListViewModel>();

        public List<Zaehlertyp> Typen => Enums.Zaehlertypen;
        public List<WohnungListEntry> Wohnungen = new List<WohnungListEntry>();

        public List<ZaehlerListEntry> EinzelZaehler;

        private ZaehlerListEntry mAllgemeinZaehler;
        public ZaehlerListEntry AllgemeinZaehler
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

        private WohnungListEntry mWohnung;
        public WohnungListEntry Wohnung
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

        private IAppImplementation Impl;

        public ZaehlerDetailViewModel(IAppImplementation ctx) : this(new Zaehler(), ctx) { }
        public ZaehlerDetailViewModel(Zaehler z, IAppImplementation impl)
        {
            Impl = impl;
            Entity = z;
            mId = Entity.ZaehlerId;

            Wohnungen = Impl.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListEntry(w, Impl))
                .ToList();

            EinzelZaehler = Impl.ctx.ZaehlerSet
               .Where(y => y.ZaehlerId != Id)
               .Select(y => new ZaehlerListEntry(y))
               .ToList();
            AllgemeinZaehler = mAllgemeinZaehler = EinzelZaehler.SingleOrDefault(y => y.Id == z.AllgemeinZaehler?.ZaehlerId);


            if (mId != 0)
            {
                Staende.Value = new ZaehlerstandListViewModel(z, impl);
                Wohnung = Wohnungen.Find(w => w.Id == z.WohnungId);
            }

            PropertyChanged += OnUpdate;

            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung = null);
        }

        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;

        public async void SelfDestruct()
        {
            Entity.Staende.ForEach(s => Impl.ctx.Zaehlerstaende.Remove(s));
            Impl.ctx.ZaehlerSet.Remove(Entity);
            Impl.SaveWalter();
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
                Impl.ctx.ZaehlerSet.Update(Entity);
            }
            else
            {
                Impl.ctx.ZaehlerSet.Add(Entity);
            }
            Impl.SaveWalter();
            if (mId != Entity.ZaehlerId)
            {
                Id = Entity.ZaehlerId;
                Staende.Value = new ZaehlerstandListViewModel(Entity, Impl);
            }
        }
    }
}
