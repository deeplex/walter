﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungDetailViewModel : BindableBase
    {
        public Betriebskostenrechnung Entity { get; }
        public int Id => Entity.BetriebskostenrechnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new ObservableProperty<int>();
        public ObservableProperty<bool> ZeigeVorlagen = new ObservableProperty<bool>();
        public ObservableProperty<WohnungListEntry> BetriebskostenrechnungsWohnung = new ObservableProperty<WohnungListEntry>();

        public async Task selfDestruct()
        {
            if (await Impl.Confirmation())
            {
                Avm.ctx.Betriebskostenrechnungen.Remove(Entity);
                Avm.SaveWalter();
            }
        }

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List;
        public List<ZaehlerListEntry> AllgemeinZaehler_List;

        private ZaehlerListEntry mAllgemeinZaehler;
        public ZaehlerListEntry AllgemeinZaehler
        {
            get => mAllgemeinZaehler;
            set
            {
                var old = mAllgemeinZaehler?.Id;
                mAllgemeinZaehler = AllgemeinZaehler_List.FirstOrDefault(e => e.Id == value?.Id);
                Entity.Zaehler = mAllgemeinZaehler?.Entity;
                RaisePropertyChangedAuto(old, value?.Id);
            }
        }

        public bool isHeizung => Entity?.Typ == Betriebskostentyp.Heizkosten;

        public double Betrag
        {
            get => Entity?.Betrag ?? 0.0;
            set
            {
                var val = double.IsNaN(value) ? 0 : value;
                var old = Entity.Betrag;
                Entity.Betrag = val;
                RaisePropertyChangedAuto(old, val);
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

        private BetriebskostentypUtil mTyp;
        public BetriebskostentypUtil Typ
        {
            get => mTyp;
            set
            {
                mTyp = value;
                var old = value.Typ;
                Entity.Typ = value.Typ;
                RaisePropertyChangedAuto(old, value.Typ);
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

        public DateTimeOffset? BetreffendesJahrDatum
        {
            get => new DateTime(BetreffendesJahr, 1, 1);
            set
            {
                BetreffendesJahr = value?.Year ?? DateTime.Now.Year - 1;
            }
        }
        public int BetreffendesJahr
        {
            get => Entity?.BetreffendesJahr ?? DateTime.Now.Year - 1;
            set
            {
                var val = value == int.MinValue ? DateTime.Now.Year - 1 : value;
                var old = Entity.BetreffendesJahr;
                Entity.BetreffendesJahr = val;
                RaisePropertyChangedAuto(old, val);
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

        public ObservableProperty<ImmutableList<WohnungListEntry>> Wohnungen
            = new ObservableProperty<ImmutableList<WohnungListEntry>>();

        public AppViewModel Avm;
        public IAppImplementation Impl;
        public void UpdateWohnungen(ImmutableList<WohnungListEntry> list)
        {
            var flagged = Wohnungen.Value.Count != list.Count;
            Wohnungen.Value = list
                .Select(e => new WohnungListEntry(e.Entity, Avm))
                .ToImmutableList();
            if (flagged) Update();
        }
        public void SaveWohnungen()
        {
            if (Entity == null) return;

            var added = new List<BetriebskostenrechnungsGruppe>();
            var removed = new List<BetriebskostenrechnungsGruppe>();

            // Wohnungen that were in through groups before
            var beforeEntity = Entity.Gruppen.Select(g => g.Wohnung);

            // Add missing Gruppen
            Wohnungen.Value
                .Where(w => !beforeEntity.Contains(w.Entity))
                .ToList()
                .ForEach(s =>
                {
                    var a = new BetriebskostenrechnungsGruppe()
                    {
                        Rechnung = Entity,
                        WohnungId = s.Id,
                    };
                    added.Add(a);
                    Avm.ctx.Betriebskostenrechnungsgruppen.Add(a);
                });

            var beforeWohnung = Wohnungen.Value.Select(w => w.Entity);

            // Remove old Gruppen
            beforeEntity.ToList().ForEach(w =>
                Avm.ctx.Betriebskostenrechnungsgruppen
                    .ToList()
                    .Where(g =>
                        g.Rechnung.BetriebskostenrechnungId == Id &&
                        g.WohnungId == w.WohnungId &&
                        !Wohnungen.Value.Exists(e => e.Id == g.WohnungId))
                    .ToList()
                    .ForEach(g =>
                    {
                        removed.Add(g);
                        Avm.ctx.Betriebskostenrechnungsgruppen.Remove(g);
                    }));
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, IAppImplementation impl, AppViewModel avm)
        {
            Entity = r;
            Avm = avm;
            Impl = impl;

            Wohnungen.Value = r.Gruppen.Select(g => new WohnungListEntry(g.Wohnung, Avm)).ToImmutableList();
            if (BetriebskostenrechnungsWohnung.Value == null)
            {
                BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            AllgemeinZaehler_List = Avm.ctx.ZaehlerSet
                .Select(a => new ZaehlerListEntry(a))
                .ToList();
            AllgemeinZaehler = AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Zaehler?.ZaehlerId);

            Typen_List = Enums.Betriebskostentyp;
            Typ = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            dispose = new AsyncRelayCommand(_ => selfDestruct());

            PropertyChanged += OnUpdate;
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, int w, List<Wohnung> l, IAppImplementation impl, AppViewModel avm) : this(r, w, impl, avm)
        {
            Wohnungen.Value = l.Select(e => new WohnungListEntry(e, avm)).ToImmutableList();
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, int w, IAppImplementation impl, AppViewModel avm) : this(r, impl, avm)
        {
            BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.Find(e => e.Id == w);
        }

        public BetriebskostenrechnungDetailViewModel(IList<WohnungListEntry> l, int betreffendesJahr, IAppImplementation impl, AppViewModel avm) : this(new Betriebskostenrechnung(), impl, avm)
        {
            var thisYear = Avm.ctx.Betriebskostenrechnungen.ToList().Where(r =>
               r.BetreffendesJahr == BetreffendesJahr - 1 &&
               r.Gruppen.Count == Wohnungen.Value.Count &&
               Wohnungen.Value.All(e => r.Gruppen.Exists(r => r.WohnungId == e.Id)))
                .ToList();

            Wohnungen.Value = l.ToImmutableList();
            BetreffendesJahr = betreffendesJahr;
        }

        public BetriebskostenrechnungDetailViewModel(IAppImplementation impl, AppViewModel avm) : this(new Betriebskostenrechnung(), impl, avm)
        {
            Entity.BetreffendesJahr = DateTime.Now.Year;
            Entity.Datum = DateTime.Now;
        }

        public AsyncRelayCommand dispose;

        public void Update()
        {
            if (Beschreibung == null || Beschreibung.Trim() == "")
            {
                return;
            }

            if (Entity.BetriebskostenrechnungId != 0)
            {
                Avm.ctx.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                Avm.ctx.Betriebskostenrechnungen.Add(Entity);
            }
            SaveWohnungen();
            Avm.SaveWalter();
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
                case nameof(Notiz):
                    break;
                default:
                    return;
            }

            Update();
        }
    }
}
