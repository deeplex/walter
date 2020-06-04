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

    public class BetriebskostenRechnungenGruppeListViewModel
    {

        public ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenGruppeListGruppe, BetriebskostenRechnungenListGruppenJahr>> Gruppen
            = new ObservableProperty<ImmutableSortedDictionary<BetriebskostenRechnungenGruppeListGruppe, BetriebskostenRechnungenListGruppenJahr>>();

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
        public BetriebskostenRechnungenGruppeListViewModel()
        {
            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new BetriebskostenRechungenListWohnungListWohnung(w))
                .GroupBy(w => w.AdresseId)
                .ToImmutableDictionary(
                    g => new BetriebskostenRechungenListWohnungListAdresse(g.Key),
                    g => g.ToImmutableList());

            Gruppen.Value = App.Walter.Betriebskostenrechnungsgruppen.ToList()
                .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .ToImmutableSortedDictionary(
                    g => new BetriebskostenRechnungenGruppeListGruppe(g.Key),
                    g => new BetriebskostenRechnungenListGruppenJahr(g.ToList())); // TODO Comparer? 
        }

        public class BetriebskostenRechnungenGruppeListGruppe
        {
            public string Bezeichnung { get; }

            public BetriebskostenRechnungenGruppeListGruppe(SortedSet<int> set)
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

        public class BetriebskostenRechnungenListGruppenJahr
        {
            public ImmutableSortedDictionary<int, ImmutableList<BetriebskostenGruppenRechnungenRechnung>> Jahre { get; }

            public BetriebskostenRechnungenListGruppenJahr(List<Betriebskostenrechnungsgruppe> r)
            {
                Jahre = r.GroupBy(gg => gg.Rechnung.BetreffendesJahr)
                    .ToImmutableSortedDictionary(
                    gg => gg.Key, gg => gg
                        .ToList()
                        .Select(ggg => new BetriebskostenGruppenRechnungenRechnung(ggg.Rechnung))
                        .ToImmutableList(),
                        Comparer<int>.Create((x, y) => y.CompareTo(x)));
            }
        }

        public class BetriebskostenGruppenRechnungenRechnung
        {
            public string Betrag { get; }
            public string Datum { get; }
            public string Typ { get; }

            public BetriebskostenGruppenRechnungenRechnung(Betriebskostenrechnung r)
            {
                Betrag = string.Format("{0:F2}€", r.Betrag);
                Datum = r.Datum.ToString("dd.MM.yyyy");
                Typ = r.Typ.ToDescriptionString();
            }
        }
    }
}
