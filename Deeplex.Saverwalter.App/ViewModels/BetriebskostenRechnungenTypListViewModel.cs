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

    public class BetriebskostenRechnungenTypListViewModel
    {
        public ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostentyp, BetriebskostenRechnungenListTypenJahr>> Typen
            = new ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenBetriebskostentyp, BetriebskostenRechnungenListTypenJahr>>();

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

        public string BetriebskostenRechnungenListGruppe(SortedSet<int> set)
        {
            var adressen = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Where(w => set.Contains(w.WohnungId))
                .GroupBy(w => w.Adresse)
                .ToDictionary(g => g.Key, g => g.ToList());
            return string.Join(" — ", adressen.Select(adr =>
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

        public List<BetriebskostenRechnungenBetriebskostentyp> Betriebskostentypen =
            Enum.GetValues(typeof(Betriebskostentyp))
                .Cast<Betriebskostentyp>()
                .ToList()
                .Select(e => new BetriebskostenRechnungenBetriebskostentyp(e))
                .ToList();

        public List<BetriebskostenRechnungenSchluessel> Betriebskostenschluessel =
            Enum.GetValues(typeof(UmlageSchluessel))
                .Cast<UmlageSchluessel>()
                .ToList()
                .Select(t => new BetriebskostenRechnungenSchluessel(t))
                .ToList();

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenTypListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ToList()
                .GroupBy(g => g.Typ)
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenBetriebskostentyp(g.Key),
                    g => new BetriebskostenRechnungenListTypenJahr(g.ToList()),
                    Comparer<BetriebskostenRechnungenBetriebskostentyp>.Create((x, y)
                        => x.Beschreibung.CompareTo(y.Beschreibung)));

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

    public class BetriebskostenRechnungenBetriebskostentyp
    {
        public Betriebskostentyp Typ { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenBetriebskostentyp(Betriebskostentyp t)
        {
            Typ = t;
            Beschreibung = t.ToDescriptionString();
        }
    }

    public class BetriebskostenRechnungenSchluessel
    {
        public UmlageSchluessel Schluessel { get; }
        public string Beschreibung { get; }
        public BetriebskostenRechnungenSchluessel(UmlageSchluessel u)
        {
            Schluessel = u;
            Beschreibung = u.ToDescriptionString();
        }
    }

    public class BetriebskostenRechungenListWohnungListAdresse : TreeViewNode
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

    public class BetriebskostenRechungenListWohnungListWohnung : TreeViewNode
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

    public class BetriebskostenRechnungenListTypenJahr
    {
        public ImmutableSortedDictionary<int, ImmutableList<BetriebskostenRechnungenRechnung>> Jahre { get; }

        public BetriebskostenRechnungenListTypenJahr(List<Betriebskostenrechnung> r)
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

    public class BetriebskostenRechnungenRechnung
    {
        public string Betrag { get; }
        public string Datum { get; }
        public ImmutableDictionary<string, ImmutableList<string>> Gruppen { get; }
        public ImmutableList<string> Wohnungen { get; }

        public BetriebskostenRechnungenRechnung(Betriebskostenrechnung r)
        {
            Betrag = string.Format("{0:F2}€", r.Betrag);
            Datum = r.Datum.ToString("dd.MM.yyyy");
            Gruppen = App.Walter.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung == r)
                .ToList()
                .GroupBy(g => g.Wohnung.Adresse)
                .ToImmutableDictionary(
                    g => AdresseViewModel.Anschrift(g.Key),
                    g => g.Select(gg => gg.Wohnung.Bezeichnung).ToImmutableList());

            Wohnungen = App.Walter.Betriebskostenrechnungsgruppen
                .Where(g => g.Rechnung == r)
                .Select(g => AdresseViewModel.Anschrift(g.Wohnung) + " — " + g.Wohnung.Bezeichnung)
                .ToImmutableList();
        }
    }
}
