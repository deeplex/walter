using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
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
        public ObservableProperty<WohnungListViewModelEntry> BetriebskostenrechnungsWohnung = new ObservableProperty<WohnungListViewModelEntry>();

        public async Task selfDestruct()
        {
            if (await NotifcationService.Confirmation())
            {
                Db.ctx.Betriebskostenrechnungen.Remove(Entity);
                Db.SaveWalter();
            }
        }

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List;
        public List<ZaehlerListViewModelEntry> AllgemeinZaehler_List;

        private ZaehlerListViewModelEntry mAllgemeinZaehler;
        public ZaehlerListViewModelEntry AllgemeinZaehler
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

        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen
            = new ObservableProperty<ImmutableList<WohnungListViewModelEntry>>();

        public IWalterDbService Db;
        public INotificationService NotifcationService;
        public void UpdateWohnungen(ImmutableList<WohnungListViewModelEntry> list)
        {
            var flagged = Wohnungen.Value.Count != list.Count;
            Wohnungen.Value = list
                .Select(e => new WohnungListViewModelEntry(e.Entity, Db))
                .ToImmutableList();
            if (flagged) Update();
        }
        public void SaveWohnungen()
        {
            if (Entity == null) return;

            // Add missing Wohnungen
            Entity.Wohnungen
                .AddRange(Wohnungen.Value.Where(w => !Entity.Wohnungen.Contains(w.Entity))
                .Select(w => w.Entity));
            // Remove old Wohnungen
            Entity.Wohnungen.RemoveAll(w => !Wohnungen.Value.Exists(v => v.Entity == w));
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, INotificationService ns, IWalterDbService db)
        {
            Entity = r;
            Db = db;
            NotifcationService = ns;

            Wohnungen.Value = r.Wohnungen.Select(g => new WohnungListViewModelEntry(g, Db)).ToImmutableList();
            if (BetriebskostenrechnungsWohnung.Value == null)
            {
                BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            AllgemeinZaehler_List = Db.ctx.ZaehlerSet
                .Select(a => new ZaehlerListViewModelEntry(a))
                .ToList();
            AllgemeinZaehler = AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Zaehler?.ZaehlerId);

            Typen_List = Enums.Betriebskostentyp;
            Typ = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            dispose = new AsyncRelayCommand(_ => selfDestruct());

            PropertyChanged += OnUpdate;
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, int w, List<Wohnung> l, INotificationService ns, IWalterDbService db) : this(r, w, ns, db)
        {
            Wohnungen.Value = l.Select(e => new WohnungListViewModelEntry(e, db)).ToImmutableList();
        }

        public BetriebskostenrechnungDetailViewModel(Betriebskostenrechnung r, int w, INotificationService ns, IWalterDbService avm) : this(r, ns, avm)
        {
            BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.Find(e => e.Id == w);
        }

        public BetriebskostenrechnungDetailViewModel(IList<WohnungListViewModelEntry> l, int betreffendesJahr, INotificationService ns, IWalterDbService avm) : this(new Betriebskostenrechnung(), ns, avm)
        {
            var thisYear = Db.ctx.Betriebskostenrechnungen.ToList().Where(r =>
               r.BetreffendesJahr == BetreffendesJahr - 1 &&
               r.Wohnungen.Count == Wohnungen.Value.Count &&
               Wohnungen.Value.All(e => r.Wohnungen.ToList().Exists(r => r.WohnungId == e.Id)))
                .ToList();

            Wohnungen.Value = l.ToImmutableList();
            BetreffendesJahr = betreffendesJahr;
        }

        public BetriebskostenrechnungDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Betriebskostenrechnung(), ns, db)
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
                Db.ctx.Betriebskostenrechnungen.Update(Entity);
            }
            else
            {
                Db.ctx.Betriebskostenrechnungen.Add(Entity);
            }
            SaveWohnungen();
            Db.SaveWalter();
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
