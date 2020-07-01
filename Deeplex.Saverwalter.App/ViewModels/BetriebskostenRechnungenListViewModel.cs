using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{

    public sealed class BetriebskostenRechnungenListViewModel
    {
        public ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostenTyp, BetriebskostenRechnungenListJahr>> Typen
            = new ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostenTyp, BetriebskostenRechnungenListJahr>>();

        public ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostenGruppe, BetriebskostenRechnungenListJahr>> Gruppen
            = new ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostenGruppe, BetriebskostenRechnungenListJahr>>();

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public ObservableProperty<bool> IsInEdit = new ObservableProperty<bool>();

        public List<BetriebskostenRechnungenBetriebskostenTyp> Betriebskostentypen =
            Enum.GetValues(typeof(Betriebskostentyp))
                .Cast<Betriebskostentyp>()
                .ToList()
                .Select(e => new BetriebskostenRechnungenBetriebskostenTyp(e))
                .ToList();

        public List<BetriebskostenRechnungenSchluessel> Betriebskostenschluessel =
            Enum.GetValues(typeof(UmlageSchluessel))
                .Cast<UmlageSchluessel>()
                .ToList()
                .Select(t => new BetriebskostenRechnungenSchluessel(t))
                .ToList();

        public List<BetriebskostenRechnungenHKVO9> HKVO9 =
            Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>()
                .ToList()
                .Select(t => new BetriebskostenRechnungenHKVO9(t))
                .ToList();

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Include(b => b.Allgemeinzaehler)
                .ToList()
                .GroupBy(g => g.Typ)
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenBetriebskostenTyp(g.Key),
                    g => new BetriebskostenRechnungenListJahr(this, g.ToList()),
                    Comparer<BetriebskostenRechnungenBetriebskostenTyp>.Create((x, y)
                        => x.Beschreibung.CompareTo(y.Beschreibung)));

            Gruppen.Value = App.Walter.Betriebskostenrechnungsgruppen.ToList()
                .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenBetriebskostenGruppe(g.Key),
                    g => new BetriebskostenRechnungenListJahr(this, g.ToList()),
                    Comparer<BetriebskostenRechnungenBetriebskostenGruppe>.Create((x, y)
                        => x.Bezeichnung.CompareTo(y.Bezeichnung)));

            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new BetriebskostenRechungenListWohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(
                    g => new BetriebskostenRechungenListWohnungListAdresse(g.Key),
                    g => g.ToImmutableList());
        }
    }

    public sealed class BetriebskostenRechnungenBetriebskostenGruppe
    {
        public string Bezeichnung { get; }

        public BetriebskostenRechnungenBetriebskostenGruppe(SortedSet<int> set)
        {
            var adressen = App.Walter.Wohnungen
            .Include(w => w.Adresse)
            .ToList()
            .Where(w => set.Contains(w.WohnungId))
            .GroupBy(w => w.Adresse)
            .ToDictionary(g => g.Key, g => g.ToList());

            Bezeichnung = string.Join(" — ", adressen.Select(adr =>
            {
                var a = adr.Key;
                var ret = a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
                if (adr.Value.Count() != a.Wohnungen.Count)
                {
                    ret += ": " + string.Join(", ", adr.Value.Select(w => w.Bezeichnung));
                }
                else
                {
                    ret += " (gesamt)";
                }
                return ret;
            }));
        }
    }


    public sealed class BetriebskostenRechnungenBetriebskostenTyp
    {
        public Betriebskostentyp Typ { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenBetriebskostenTyp(Betriebskostentyp t)
        {
            Typ = t;
            Beschreibung = t.ToDescriptionString();
        }
    }

    public sealed class BetriebskostenRechnungenSchluessel
    {
        public UmlageSchluessel Schluessel { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenSchluessel(UmlageSchluessel u)
        {
            Schluessel = u;
            Beschreibung = u.ToDescriptionString();
        }
    }

    public sealed class BetriebskostenRechnungenHKVO9
    {
        public HKVO_P9A2 Enum { get; }
        public string Absatz { get; }
        public BetriebskostenRechnungenHKVO9(HKVO_P9A2 h)
        {
            Enum = h;
            Absatz = "Absatz " + ((int)h).ToString();
        }
    }

    public sealed class BetriebskostenRechungenListWohnungListAdresse : TreeViewNode
    {
        public int Id { get; }
        public string Anschrift { get; }
        public BetriebskostenRechungenListWohnungListAdresse(int id)
        {
            Id = id;
            Anschrift = AdresseViewModel.Anschrift(id);
            Content = Anschrift;
        }
    }

    public sealed class BetriebskostenRechungenListWohnungListWohnung : TreeViewNode
    {
        public int Id { get; }
        public int AdresseId { get; }
        public string Bezeichnung { get; }


        public BetriebskostenRechungenListWohnungListWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            AdresseId = w.AdresseId;
            Bezeichnung = w.Bezeichnung;
            Content = Bezeichnung;
        }
    }

    public sealed class BetriebskostenRechnungenListJahr
    {
        public ImmutableSortedDictionary<int, ImmutableList<BetriebskostenRechnungenRechnung>> Jahre { get; }

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, ImmutableSortedDictionary<int, ImmutableList<BetriebskostenRechnungenRechnung>> j)
        {
            Jahre = j;
        }

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, List<BetriebskostenrechnungsGruppe> r)
        {
            Jahre = r.GroupBy(gg => gg.Rechnung.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                gg => gg.Key, gg => gg
                    .ToList()
                    .Select(ggg => new BetriebskostenRechnungenRechnung(t, ggg.Rechnung))
                    .ToImmutableList(),
                    Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, List<Betriebskostenrechnung> r)
        {
            Jahre = r.GroupBy(gg => gg.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                gg => gg.Key, gg => gg
                    .ToList()
                    .Select(ggg => new BetriebskostenRechnungenRechnung(t, ggg))
                    .ToImmutableList(),
                    Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }
    }

    public sealed class BetriebskostenRechnungenRechnung : BindableBase
    {
        private Betriebskostenrechnung Entity { get; set; }
        public void AddEntity(Betriebskostenrechnung r)
        {
            Entity = r;
        }

        public bool hasNoEntity => Entity == null;
        public double Betrag
        {
            get => Entity.Betrag;
            set
            {
                Entity.Betrag = value;
                RaisePropertyChangedAuto();
            }
        }

        public string Beschreibung { get; }
        public int BetreffendesJahr { get; }
        public Betriebskostentyp Typ { get; }

        public bool isHeizung => Typ == Betriebskostentyp.Heizkosten;

        public double HKVO_P7
        {
            get => (Entity.HKVO_P7 ?? 0) * 100;
            set
            {
                Entity.HKVO_P7 = value / 100;
                RaisePropertyChangedAuto();
            }
        }
        public double HKVO_P8
        {
            get => (Entity.HKVO_P8 ?? 0) * 100;
            set
            {
                Entity.HKVO_P8 = value / 100;
                RaisePropertyChangedAuto();
            }
        }

        public List<BetriebskostenrechnungHKVO_P9A2> HKVO_P9_List =
            Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>().ToList()
                .Select(s => new BetriebskostenrechnungHKVO_P9A2(s))
                .ToList();

        public sealed class BetriebskostenrechnungHKVO_P9A2
        {
            public int index { get; }
            public string text { get; }

            public BetriebskostenrechnungHKVO_P9A2(HKVO_P9A2 S)
            {
                index = (int)S;
                text = "Satz " + index.ToString();
            }
        }

        public int HKVO_P9
        {
            get => HKVO_P9_List.FindIndex(i => i.index == (int)Entity.HKVO_P9);
            set
            {
                Entity.HKVO_P9 = (HKVO_P9A2)HKVO_P9_List[value].index;
                RaisePropertyChangedAuto();
            }
        }

        public UmlageSchluessel Schluessel { get; }
        public DateTimeOffset Datum {
            get => Entity.Datum;
            set
            {
                Entity.Datum = value.Date.AsUtcKind();
                RaisePropertyChangedAuto();
            }
        }
        public string Notiz { get; }
        public ImmutableList<string> Wohnungen { get; }
        public ImmutableList<int> WohnungenIds { get; }

        public ObservableProperty<bool> IsInEdit;
        //public ImmutableDictionary<string, ImmutableList<string>> Gruppen { get; }

        public BetriebskostenRechnungenRechnung(BetriebskostenRechnungenListViewModel t, BetriebskostenRechnungenRechnung r)
        {
            // Template for next year (Note: Entity is null here)
            BetreffendesJahr = r.BetreffendesJahr + 1;
            Datum = r.Datum.AddYears(1);
            Betrag = 0;
            Beschreibung = r.Beschreibung;
            Notiz = r.Notiz;
            Schluessel = r.Schluessel;
            Typ = r.Typ;

            Wohnungen = r.Wohnungen;
            WohnungenIds = r.WohnungenIds;

            IsInEdit = t.IsInEdit;
            PropertyChanged += OnUpdate;
        }
        public BetriebskostenRechnungenRechnung(BetriebskostenRechnungenListViewModel t, Betriebskostenrechnung r)
        {
            Entity = r;
            Betrag = r.Betrag;
            Datum = r.Datum.AsUtcKind();
            Beschreibung = r.Beschreibung;
            BetreffendesJahr = r.BetreffendesJahr;
            Notiz = r.Notiz;
            Schluessel = r.Schluessel;
            Typ = r.Typ;

            var w = App.Walter.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung == r);

            WohnungenIds = w.Select(g => g.WohnungId).ToImmutableList();

            Wohnungen = w.Select(g => AdresseViewModel.Anschrift(g.Wohnung) + " — " + g.Wohnung.Bezeichnung)
                .ToImmutableList();

            AttachFile = new AsyncRelayCommand(async _ =>
                await Utils.Files.SaveFilesToWalter(App.Walter.BetriebskostenrechnungAnhaenge, r), _ => true);

            IsInEdit = t.IsInEdit;

            PropertyChanged += OnUpdate;
        }

        public AsyncRelayCommand AttachFile;

        private void OnUpdate(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // case Gruppen
                //case nameof(Notiz):
                case nameof(Betrag):
                case nameof(Datum):
                case nameof(HKVO_P7):
                case nameof(HKVO_P8):
                case nameof(HKVO_P9):
                //case nameof(AllgemeinZaehler):
                    break;
                default:
                    return;
            }

            if (Entity.Datum == null || (
                Typ == Betriebskostentyp.Heizkosten && (
                    Entity.HKVO_P7 == null ||
                    Entity.HKVO_P8 == null ||
                    Entity.HKVO_P9 == null ||
                    Entity.Allgemeinzaehler == null)))
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
            App.Walter.SaveChanges();
        }

        // Sorting after Adress? Tree View sth?
        //Gruppen = App.Walter.Betriebskostenrechnungsgruppen
        //    .Where(g => g.Rechnung == r)
        //    .ToList()
        //    .GroupBy(g => g.Wohnung.Adresse)
        //    .ToImmutableDictionary(
        //        g => AdresseViewModel.Anschrift(g.Key),
        //        g => g.Select(gg => gg.Wohnung.Bezeichnung).ToImmutableList());
    }
}
