﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
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

        public List<Zaehler> EinzelZaehler =>
            App.Walter.ZaehlerSet.Where(z => z.ZaehlerId != Id).ToList();

        public Zaehler AllgemeinZaehler
        {
            get => Entity.AllgemeinZaehler;
            set
            {
                var old = Entity.AllgemeinZaehler;
                Entity.AllgemeinZaehler = value;
                RaisePropertyChangedAuto(old, value);
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

        public ZaehlerDetailViewModel() : this(new Zaehler()) { }

        public ZaehlerDetailViewModel(Zaehler z)
        {
            Entity = z;
            mId = Entity.ZaehlerId;

            Wohnungen = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListEntry(w))
                .ToList();

            if (mId != 0)
            {
                Staende.Value = new ZaehlerstandListViewModel(z);
                Wohnung = Wohnungen.Find(w => w.Id == z.WohnungId);
            }
            
            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung = null);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;

        public async void SelfDestruct()
        {
            Entity.Staende.ForEach(s => App.Walter.Zaehlerstaende.Remove(s));
            App.Walter.ZaehlerSet.Remove(Entity);
            App.SaveWalter();
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
                App.Walter.ZaehlerSet.Update(Entity);
            }
            else
            {
                App.Walter.ZaehlerSet.Add(Entity);
            }
            App.SaveWalter();
            if (mId != Entity.ZaehlerId)
            {
                Id = Entity.ZaehlerId;
                Staende.Value = new ZaehlerstandListViewModel(Entity);
            }
        }
    }
}
