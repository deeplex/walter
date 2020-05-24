using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{

    public class BetriebskostenRechnungenListViewModel
    {
        public ObservableProperty<ImmutableDictionary<Betriebskostentyp, BetriebskostenRechnungenListTyp>> Typen
            = new ObservableProperty<ImmutableDictionary<Betriebskostentyp, BetriebskostenRechnungenListTyp>>();

        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;

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
        public BetriebskostenRechnungenListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungsgruppen
                .ToList()
                .GroupBy(g => g.Rechnung.Typ)
                .ToImmutableDictionary(g => g.Key, g => new BetriebskostenRechnungenListTyp(g.ToList()));


            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new BetriebskostenRechungenListWohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(g => new BetriebskostenRechungenListWohnungListAdresse(g.Key), g => g.ToImmutableList());
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

    public class BetriebskostenRechnungenListTyp
    {
        public ObservableProperty<ImmutableSortedDictionary<int, BetriebskostenRechnungenListTypenJahr>> Jahre
            = new ObservableProperty<ImmutableSortedDictionary<int, BetriebskostenRechnungenListTypenJahr>>();


        public BetriebskostenRechnungenListTyp(List<Betriebskostenrechnungsgruppe> g)
        {
            Jahre.Value = g
                .GroupBy(gg => gg.Rechnung.Datum.Year)
                .ToImmutableSortedDictionary(gg => gg.Key, gg => new BetriebskostenRechnungenListTypenJahr(gg.ToList()));
        }
    }

    public class BetriebskostenRechnungenListTypenJahr
    {
        public ObservableProperty<ImmutableDictionary<string, BetriebskostenRechnungenGruppe>> Gruppe
            = new ObservableProperty<ImmutableDictionary<string, BetriebskostenRechnungenGruppe>>();

        public ObservableProperty<double> Betrag = new ObservableProperty<double>();
        public ObservableProperty<DateTimeOffset> Datum = new ObservableProperty<DateTimeOffset>();
        public BetriebskostenRechnungenListTypenJahr(List<Betriebskostenrechnungsgruppe> g)
        {
            Gruppe.Value = g
                .GroupBy(gg => gg.Wohnung.Adresse)
                .ToImmutableDictionary(
                    gg => AdresseViewModel.Anschrift(gg.Key),
                    gg => new BetriebskostenRechnungenGruppe(gg.ToList()));

            Betrag.Value = g.First().Rechnung.Betrag;
            Datum.Value = g.First().Rechnung.Datum;
        }
    }

    public class BetriebskostenRechnungenGruppe
    {
        public ObservableProperty<ImmutableDictionary<string, ImmutableList<BetriebskostenRechnungenWohnung>>> Anschriften
            = new ObservableProperty<ImmutableDictionary<string, ImmutableList<BetriebskostenRechnungenWohnung>>>();

        public BetriebskostenRechnungenGruppe(List<Betriebskostenrechnungsgruppe> g)
        {
            Anschriften.Value = g
                .GroupBy(gg => gg.Wohnung.Adresse)
                .ToImmutableDictionary(gg => AdresseViewModel.Anschrift(gg.Key), gg => gg
                    .Select(ggg => new BetriebskostenRechnungenWohnung(ggg))
                    .ToImmutableList());
        }
    }

    public class BetriebskostenRechnungenWohnung
    {
        public string Bezeichnung { get; }
        public BetriebskostenRechnungenWohnung(Betriebskostenrechnungsgruppe g)
        {
            Bezeichnung = g.Wohnung.Bezeichnung;
        }
    }
}
