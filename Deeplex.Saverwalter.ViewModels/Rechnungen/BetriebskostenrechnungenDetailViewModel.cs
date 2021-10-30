using Deeplex.Saverwalter.Model;
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

        private async void fromLastYear()
        {
            var lastYear = Avm.ctx.Betriebskostenrechnungen.ToList().Where(r =>
                r.BetreffendesJahr == BetreffendesJahr - 1 &&
                r.Gruppen.Count == Wohnungen.Value.Count &&
                Wohnungen.Value.All(e => r.Gruppen.Exists(r => r.WohnungId == e.Id)))
            .ToList().Find(f => f.Typ == mTyp.Typ);

            if (lastYear == null ||
                !await Impl.Confirmation(
                    "Daten aus dem Vorjahr übernehmen?",
                    "Sollen für die Betriebskostenrechnung " +
                    Typ.Typ.ToDescriptionString() +
                    " die Werte aus dem Vorjahr (" + (BetreffendesJahr - 1).ToString() + ") übernommen werden?",
                    "Ja", "Nein"))
            {
                return;
            }

            Beschreibung = lastYear.Beschreibung;
            Betrag = lastYear.Betrag;
            Schluessel = (int)lastYear.Schluessel;
            Notiz = lastYear.Notiz;
            Datum = lastYear.Datum.AddYears(1);
            if (lastYear.HKVO_P7 is double p7) HKVO_P7 = p7;
            if (lastYear.HKVO_P8 is double p8) HKVO_P8 = p8;
            if (lastYear.HKVO_P9 is HKVO_P9A2 p9) HKVO_P9 = (int)p9;
            if (lastYear.Zaehler is Zaehler z) AllgemeinZaehler = new ZaehlerListEntry(z);
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
                fromLastYear();
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
            var before = Wohnungen.Value;
            Wohnungen.Value = list
                .Select(e => new WohnungListEntry(e.Entity, Avm))
                .ToImmutableList();
            SaveWohnungen(before);
            Update();
        }
        public void SaveWohnungen(ImmutableList<WohnungListEntry> before)
        {
            // Add missing Gruppen
            before
                .Where(s => !Wohnungen.Value.Exists(w => w.Id == (s as WohnungListEntry).Id))
                .ToList()
                .ForEach(s =>
                {
                    Avm.ctx.Betriebskostenrechnungsgruppen.Add(new BetriebskostenrechnungsGruppe()
                    {
                        Rechnung = Avm.ctx.Betriebskostenrechnungen.Find(Id),
                        WohnungId = (s as WohnungListEntry).Id,
                    });
                });

            // Remove old Gruppen
            Wohnungen.Value
                .Where(w => !before.Exists(s => w.Id == (s as WohnungListEntry).Id))
                .ToList()
                .ForEach(w =>
                {
                    Avm.ctx.Betriebskostenrechnungsgruppen
                        .Where(g => g.Rechnung.BetriebskostenrechnungId == Id && g.WohnungId == w.Id)
                        .ToList()
                        .ForEach(g => Avm.ctx.Betriebskostenrechnungsgruppen.Remove(g));
                });
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, IAppImplementation impl, AppViewModel avm)
        {
            Entity = r;
            Avm = avm;
            Impl = impl;

            Wohnungen.Value = r.Gruppen.Select(g => new WohnungListEntry(g.Wohnung, Avm)).ToImmutableList();

            AllgemeinZaehler_List = Avm.ctx.ZaehlerSet
                .Select(a => new ZaehlerListEntry(a))
                .ToList();
            AllgemeinZaehler = AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Zaehler?.ZaehlerId);

            Typen_List = Enums.Betriebskostentyp;
            Typ = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            dispose = new AsyncRelayCommand(_ => selfDestruct());

            PropertyChanged += OnUpdate;
        }

        public BetriebskostenrechnungDetailViewModel(IList<WohnungListEntry> l, int betreffendesJahr, IAppImplementation impl, AppViewModel avm) : this(new Betriebskostenrechnung(), impl, avm)
        {
            //Typ = Typen_List.FirstOrDefault(w => w.Typ == missing.First().Typ);
            Wohnungen.Value = l.ToImmutableList();
            BetreffendesJahr = betreffendesJahr;
            Entity.Datum = DateTime.Now;
        }

        public BetriebskostenrechnungDetailViewModel(IAppImplementation impl, AppViewModel avm) : this(new Betriebskostenrechnung(), impl, avm)
        {
            Entity.BetreffendesJahr = DateTime.Now.Year;
            Entity.Datum = DateTime.Now;
        }

        public AsyncRelayCommand dispose;

        public void Update()
        {
            if (Entity.BetriebskostenrechnungId != 0)
            {
                Avm.ctx.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                Avm.ctx.Betriebskostenrechnungen.Add(Entity);
            }
            var before = Wohnungen.Value;
            SaveWohnungen(before);
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
