using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ToList()
                .GroupBy(g => g.Typ)
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenBetriebskostenTyp(g.Key),
                    g => new BetriebskostenRechnungenListJahr(g.ToList()),
                    Comparer<BetriebskostenRechnungenBetriebskostenTyp>.Create((x, y)
                        => x.Beschreibung.CompareTo(y.Beschreibung)));

            Gruppen.Value = App.Walter.Betriebskostenrechnungsgruppen.ToList()
                .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenBetriebskostenGruppe(g.Key),
                    g => new BetriebskostenRechnungenListJahr(g.ToList()),
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

        public BetriebskostenRechnungenListJahr(ImmutableSortedDictionary<int, ImmutableList<BetriebskostenRechnungenRechnung>> j)
        {
            Jahre = j;
        }

        public BetriebskostenRechnungenListJahr(List<Betriebskostenrechnungsgruppe> r)
        {
            Jahre = r.GroupBy(gg => gg.Rechnung.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                gg => gg.Key, gg => gg
                    .ToList()
                    .Select(ggg => new BetriebskostenRechnungenRechnung(ggg.Rechnung))
                    .ToImmutableList(),
                    Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }

        public BetriebskostenRechnungenListJahr(List<Betriebskostenrechnung> r)
        {
            Jahre = r.GroupBy(gg => gg.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                gg => gg.Key, gg => gg
                    .ToList()
                    .Select(ggg => new BetriebskostenRechnungenRechnung(ggg))
                    .ToImmutableList(),
                    Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }
    }

    public sealed class BetriebskostenRechnungenRechnung
    {
        private Betriebskostenrechnung Entity { get; set; }
        public void AddEntity(Betriebskostenrechnung r)
        {
            Entity = r;
        }

        public bool hasNoEntity => Entity == null;
        public double Betrag { get; }
        public string Beschreibung { get; }
        public int BetreffendesJahr { get; }
        public string BetragString => string.Format("{0:F2}€", Betrag);
        public Betriebskostentyp Typ { get; }
        public string DatumString => Datum.ToString("dd.MM.yyyy");
        public UmlageSchluessel Schluessel { get; }
        public DateTimeOffset Datum { get; set; }
        public string Notiz { get; }
        public ImmutableList<string> Wohnungen { get; }
        public ImmutableList<int> WohnungenIds { get; }
        //public ImmutableDictionary<string, ImmutableList<string>> Gruppen { get; }

        public BetriebskostenRechnungenRechnung(BetriebskostenRechnungenRechnung r)
        {
            // Template for next year (Note: Entity is null here)
            BetreffendesJahr = r.BetreffendesJahr + 1;
            Datum = r.Datum.AddYears(1);
            Betrag = r.Betrag;
            Beschreibung = r.Beschreibung;
            Notiz = r.Notiz;
            Schluessel = r.Schluessel;
            Typ = r.Typ;

            Wohnungen = r.Wohnungen;
            WohnungenIds = r.WohnungenIds;
        }
        public BetriebskostenRechnungenRechnung(Betriebskostenrechnung r)
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
        }

        public AsyncRelayCommand AttachFile;

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
