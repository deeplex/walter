using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungDetailViewModel : BindableBase, IDetail
    {
        public override string ToString() => Entity.Typ.ToDescriptionString() + " - " + Entity.GetWohnungenBezeichnung();

        public Betriebskostenrechnung Entity { get; }
        public int Id => Entity.BetriebskostenrechnungId;

        public ObservableProperty<int> BetriebskostenrechnungsJahr = new();
        public ObservableProperty<bool> ZeigeVorlagen = new();
        public ObservableProperty<WohnungListViewModelEntry> BetriebskostenrechnungsWohnung = new();

        public List<HKVO9Util> HKVO_P9_List = Enums.HKVO9;
        public List<UmlageSchluesselUtil> Schluessel_List = Enums.UmlageSchluessel;
        public List<BetriebskostentypUtil> Typen_List = Enums.Betriebskostentyp;
        public List<ZaehlerListViewModelEntry> AllgemeinZaehler_List;

        public SavableProperty<ZaehlerListViewModelEntry> AllgemeinZaehler { get; }

        public bool isHeizung => Entity?.Typ == Betriebskostentyp.Heizkosten;

        public SavableProperty<double> Betrag { get; }
        public SavableProperty<DateTimeOffset> Datum { get; }
        public SavableProperty<string> Notiz { get; }
        public SavableProperty<BetriebskostentypUtil> Typ { get; }
        public SavableProperty<UmlageSchluesselUtil> Schluessel { get; }
        public SavableProperty<string> Beschreibung { get; }
        public DateTimeOffset? BetreffendesJahrDatum
        {
            get => new DateTime(BetreffendesJahr.Value, 1, 1);
            set
            {
                BetreffendesJahr.Value = value?.Year ?? DateTime.Now.Year - 1;
            }
        }
        public SavableProperty<int> BetreffendesJahr { get; }
        public SavableProperty<double> HKVO_P7 { get; }
        public SavableProperty<double> HKVO_P8 { get; }
        public SavableProperty<HKVO_P9A2?> HKVO_P9 { get; }

        public ObservableProperty<ImmutableList<WohnungListViewModelEntry>> Wohnungen = new();

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

            Betrag = new(this, r.Betrag);
            Datum = new(this, r.Datum.AsUtcKind());
            Notiz = new(this, r.Notiz);
            Typ = new(this, new(r.Typ));
            Beschreibung = new(this, r.Beschreibung);
            BetreffendesJahr = new(this, r.BetreffendesJahr);
            HKVO_P7 = new(this, r.HKVO_P7 ?? 0);
            HKVO_P8 = new(this, r.HKVO_P8 ?? 0);
            HKVO_P9 = new(this, r.HKVO_P9);
            Schluessel = new(this, new(r.Schluessel));
            Schluessel.Value = Schluessel_List.FirstOrDefault(e => e.Schluessel == r.Schluessel);
            Typ.Value = Typen_List.FirstOrDefault(e => e.Typ == r.Typ);

            Wohnungen.Value = r.Wohnungen.Select(g => new WohnungListViewModelEntry(g, Db)).ToImmutableList();
            if (BetriebskostenrechnungsWohnung.Value == null)
            {
                BetriebskostenrechnungsWohnung.Value = Wohnungen.Value.FirstOrDefault();
            }

            AllgemeinZaehler_List = Db.ctx.ZaehlerSet
                .Select(a => new ZaehlerListViewModelEntry(a))
                .ToList();
            AllgemeinZaehler = new(this, AllgemeinZaehler_List.FirstOrDefault(e => e.Id == r.Zaehler?.ZaehlerId));

            Delete = new AsyncRelayCommand(async _ =>
            {
                if (await NotifcationService.Confirmation())
                {
                    Db.ctx.Betriebskostenrechnungen.Remove(Entity);
                    Db.SaveWalter();
                }
            });

            Save = new RelayCommand(_ => save(), _ => true);
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
               r.BetreffendesJahr == BetreffendesJahr.Value - 1 &&
               r.Wohnungen.Count == Wohnungen.Value.Count &&
               Wohnungen.Value.All(e => r.Wohnungen.ToList().Exists(r => r.WohnungId == e.Id)))
                .ToList();

            Wohnungen.Value = l.ToImmutableList();
            BetreffendesJahr.Value = betreffendesJahr;
        }

        public BetriebskostenrechnungDetailViewModel(INotificationService ns, IWalterDbService db) : this(new Betriebskostenrechnung(), ns, db)
        {
            Entity.BetreffendesJahr = DateTime.Now.Year;
            Entity.Datum = DateTime.Now;
        }

        public RelayCommand Save { get; }
        public AsyncRelayCommand Delete { get; }

        public void Update()
        {
            if (Beschreibung == null || Beschreibung.Value.Trim() == "")
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
            NotifcationService.outOfSync = false;
        }

        public bool checkNullable<T>(object a, T b)
        {
            if (a == null && b.Equals(default(T)))
            {
                return false;
            }
            else
            {
                return a.Equals(b);
            }

        }

        public void checkForChanges()
        {
            NotifcationService.outOfSync =
                Entity.Betrag != Betrag.Value ||
                Entity.Datum.AsUtcKind() != Datum.Value ||
                Entity.Notiz != Notiz.Value ||
                Entity.Typ != Typ.Value.Typ ||
                Entity.Schluessel != Schluessel.Value.Schluessel ||
                Entity.Beschreibung != Beschreibung.Value ||
                Entity.BetreffendesJahr != BetreffendesJahr.Value ||
                checkNullable(Entity.HKVO_P7, HKVO_P7.Value) ||
                checkNullable(Entity.HKVO_P8, HKVO_P8.Value) ||
                checkNullable(Entity.HKVO_P9, HKVO_P9.Value);
        }

        private void save()
        {
            Entity.Betrag = Betrag.Value;
            Entity.Datum = Datum.Value.UtcDateTime;
            Entity.Notiz = Notiz.Value;
            Entity.Typ = Typ.Value.Typ;
            Entity.Schluessel = Schluessel.Value.Schluessel;
            Entity.Beschreibung = Beschreibung.Value;
            Entity.BetreffendesJahr = BetreffendesJahr.Value;
            if (Entity.HKVO_P7 != null && HKVO_P7.Value != 0)
            {
                Entity.HKVO_P7 = HKVO_P7.Value;
            }
            if (Entity.HKVO_P8 != null && HKVO_P8.Value != 0)
            {
                Entity.HKVO_P8 = HKVO_P8.Value;
            }
            if (Entity.HKVO_P9 != null && HKVO_P9.Value != 0)
            {
                Entity.HKVO_P9 = HKVO_P9.Value;
            }

            Update();
        }
    }
}
