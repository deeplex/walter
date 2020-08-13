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
        public SortedSet<int> WohnungIds { get; }

        public BetriebskostenRechnungenBetriebskostenGruppe(SortedSet<int> set)
        {
            WohnungIds = set;

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
        public ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<int>>> Jahre { get; set; }
            = new ObservableProperty<ImmutableSortedDictionary<int, ImmutableList<int>>>();

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, ImmutableSortedDictionary<int, ImmutableList<int>> JahrRechnungIds)
        {
            Jahre.Value = JahrRechnungIds;
        }

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, List<BetriebskostenrechnungsGruppe> r)
        {
            Jahre.Value = r.GroupBy(gg => gg.Rechnung.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                    gg => gg.Key, gg => gg.ToList()
                        .Select(ggg => ggg.Rechnung.BetriebskostenrechnungId).Distinct()
                        .ToImmutableList(),
                        Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }

        public BetriebskostenRechnungenListJahr(BetriebskostenRechnungenListViewModel t, List<Betriebskostenrechnung> r)
        {
            Jahre.Value = r.GroupBy(gg => gg.BetreffendesJahr)
                .ToImmutableSortedDictionary(
                    gg => gg.Key, gg => gg.ToList()
                        .Select(ggg => ggg.BetriebskostenrechnungId)
                        .ToImmutableList(),
                        Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }
    }
}
