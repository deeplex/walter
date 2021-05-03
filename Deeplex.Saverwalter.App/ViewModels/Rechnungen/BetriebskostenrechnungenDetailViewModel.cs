﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class BetriebskostenrechnungDetailViewModel : BindableBase
    {
        private Betriebskostenrechnung Entity { get; }
        public int Id => Entity.BetriebskostenrechnungId;

        public async void selfDestruct()
        {
            App.Walter.Betriebskostenrechnungen.Remove(Entity);
            App.SaveWalter();
        }

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;
        public List<BetriebskostenrechnungAllgemeinZaehler> AllgemeinZaehler_List
            = App.Walter.ZaehlerSet
            .Select(a => new BetriebskostenrechnungAllgemeinZaehler(a))
            .ToList();

        public sealed class BetriebskostenrechnungAllgemeinZaehler
        {
            public int Id { get; }
            public string Kennnummer { get; }

            public BetriebskostenrechnungAllgemeinZaehler(Zaehler a)
            {
                Id = a.ZaehlerId;
                Kennnummer = a.Kennnummer;
            }
        }

        public int AllgemeinZaehler
        {
            get => Entity?.Zaehler != null ?
                AllgemeinZaehler_List.FindIndex(a => a.Id == Entity.Zaehler.ZaehlerId) : 0;
            set
            {
                // TODO yeah...
                Entity.Zaehler = App.Walter.ZaehlerSet.Find(AllgemeinZaehler_List[value].Id);
                RaisePropertyChangedAuto();
            }
        }

        public bool isHeizung => Entity?.Typ == Betriebskostentyp.Heizkosten;

        public string BetragString => Betrag.ToString() + "€";
        public double Betrag
        {
            get => Entity?.Betrag ?? 0.0;
            set
            {
                var old = Entity.Betrag;
                Entity.Betrag = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public DateTimeOffset? Datum
        {
            get => Entity.Datum.AsUtcKind();
            set
            {
                var old = Entity.Datum;
                Entity.Datum = value.Value.Date.AsUtcKind();
                RaisePropertyChangedAuto(old, value.Value.Date.AsUtcKind());
            }
        }

        public string TypString => Typen_List[Typ].Beschreibung;
        public int Typ
        {
            get => Typen_List.FindIndex(i => i.index == (Entity != null ? (int)Entity?.Typ : 0));
            set
            {
                // TODO yeah...
                Entity.Typ = (Betriebskostentyp)Typen_List[value].index;
                RaisePropertyChangedAuto();
            }
        }

        public int Schluessel
        {
            get => Entity != null ? (int)Entity.Schluessel : 0;
            set
            {
                // TODO yeah...
                Entity.Schluessel = (UmlageSchluessel)value;
                RaisePropertyChangedAuto();
            }
        }

        public string Beschreibung
        {
            get => Entity?.Beschreibung ?? "";
            set
            {
                var old = Entity.Beschreibung;
                Entity.Beschreibung = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public string BetreffendesJahrString => BetreffendesJahr.ToString();
        public int BetreffendesJahr
        {
            get => Entity?.BetreffendesJahr ?? DateTime.Now.Year;
            set
            {
                var old = Entity.BetreffendesJahr;
                Entity.BetreffendesJahr = value;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double HKVO_P7
        {
            get => (Entity?.HKVO_P7 ?? 0.0) * 100;
            set
            {
                var old = Entity.HKVO_P7 / 100;
                Entity.HKVO_P7 = value / 100;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public double HKVO_P8
        {
            get => (Entity?.HKVO_P8 ?? 0.0) * 100;
            set
            {
                var old = Entity.HKVO_P8 / 100;
                Entity.HKVO_P8 = value / 100;
                RaisePropertyChangedAuto(old, value);
            }
        }

        public int HKVO_P9
        {
            get
            {
                if (Entity == null)
                {
                    return 0;
                }
                else if (Entity.Typ == Betriebskostentyp.Heizkosten)
                {
                    var index = Entity.HKVO_P9 == null ? 0 : (int)Entity.HKVO_P9;
                    return HKVO_P9_List.FindIndex(i => i.index == index);
                }
                return 0;
            }
            set
            {
                // TODO yeah...
                Entity.HKVO_P9 = (HKVO_P9A2)HKVO_P9_List[value].index;
                RaisePropertyChangedAuto();
            }
        }

        public List<Wohnung> Wohnungen { get; }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r)
        {
            Entity = r;
            
            Wohnungen = r.Gruppen.Select(g => g.Wohnung).ToList();

            AttachFile = new AsyncRelayCommand(async _ =>
                /* TODO */await Task.FromResult<object>(null), _ => false);

            dispose = new RelayCommand(_ => selfDestruct());

            PropertyChanged += OnUpdate;

            AttachFile = new AsyncRelayCommand(async _ =>
                await Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, r), _ => true);

        }

        public BetriebskostenrechnungDetailViewModel() : this(new Betriebskostenrechnung())
        {
            Entity.BetreffendesJahr = DateTime.Now.Year;
            Entity.Datum = DateTime.Now;
        }

        public AsyncRelayCommand AttachFile;
        public RelayCommand dispose;

        public void Update()
        {
            if (Beschreibung == "" || Beschreibung == null || Entity.Datum == null)
            {
                return;
            }

            if (Entity.BetriebskostenrechnungId != 0)
            {
                App.Walter.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                App.Walter.Betriebskostenrechnungen.Add(Entity);
            }
            App.SaveWalter();
        }


        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Betrag):
                case nameof(Datum):
                case nameof(Typ):
                case nameof(Schluessel):
                case nameof(Beschreibung):
                case nameof(BetreffendesJahr):
                case nameof(HKVO_P7):
                case nameof(HKVO_P8):
                    break;
                default:
                    return;
            }

            Update();
        }
    }
}
