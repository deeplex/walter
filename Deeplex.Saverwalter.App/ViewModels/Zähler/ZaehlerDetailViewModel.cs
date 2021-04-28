using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class ZaehlerDetailViewModel : BindableBase
    {
        public Zaehler Entity;
        public int Id => Entity.ZaehlerId;

        public List<Zaehlertyp> Typen => Enums.Zaehlertypen;
        public List<ZaehlerWohnung> Wohnungen = new List<ZaehlerWohnung>();

        public List<Zaehler> EinzelZaehler =>
            App.Walter.ZaehlerSet.Where(z => z.ZaehlerId != Id).ToList();

        public Zaehler AllgemeinZaehler
        {
            get => Entity.AllgemeinZaehler;
            set
            {
                Entity.AllgemeinZaehler = value;
                RaisePropertyChangedAuto();
            }
        }

        public Zaehlertyp Typ
        {
            get => Entity.Typ;
            set
            {
                Entity.Typ = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Kennnummer
        {
            get => Entity.Kennnummer;
            set
            {
                Entity.Kennnummer = value;
                RaisePropertyChangedAuto();
            }
        }

        private ZaehlerWohnung mWohnung;
        public ZaehlerWohnung Wohnung
        {
            get => mWohnung;
            set
            {
                Entity.Wohnung = value?.Entity;
                mWohnung = value;
                RaisePropertyChangedAuto();
            }
        }

        // Necessary to show / hide Zählerstände
        public bool Initialized => Entity.ZaehlerId != 0;

        public ZaehlerDetailViewModel() : this(new Zaehler()) { }

        public ZaehlerDetailViewModel(Zaehler z)
        {
            Entity = z;
            Wohnungen = App.Walter.Wohnungen.Select(w => new ZaehlerWohnung(w)).ToList();
            Wohnung = Wohnungen.Find(w => w.Id == z.WohnungId);

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.ZaehlerAnhaenge, z), _ => true);
            DeleteAllgemeinZaehler = new RelayCommand(_ => AllgemeinZaehler = null);
            DeleteZaehlerWohnung = new RelayCommand(_ => Wohnung = null);
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand DeleteAllgemeinZaehler;
        public RelayCommand DeleteZaehlerWohnung;

        public void SelfDestruct()
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

            if (Entity.Kennnummer == "")
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
            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(Initialized));
        }

        public class ZaehlerWohnung
        {
            internal Wohnung Entity;
            internal int Id => Entity.WohnungId;
            public string Anschrift { get; }

            public ZaehlerWohnung(Wohnung w)
            {
                Entity = w;
                Anschrift = AdresseViewModel.Anschrift(w) + ", " + w.Bezeichnung;
            }
        }
    }
}
