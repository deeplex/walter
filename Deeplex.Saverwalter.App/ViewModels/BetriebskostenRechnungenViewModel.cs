using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
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

        public BetriebskostenRechnungenListViewModel()
        {
            Typen.Value = App.Walter.Betriebskostenrechnungsgruppen
                .GroupBy(g => g.Rechnung.Typ)
                .ToImmutableDictionary(g => g.Key, g => new BetriebskostenRechnungenListTyp(g.ToList()));
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
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();
        public BetriebskostenRechnungenWohnung(Betriebskostenrechnungsgruppe g)
        {
            Bezeichnung.Value = g.Wohnung.Bezeichnung;
        }
    }
}
