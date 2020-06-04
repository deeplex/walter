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

            public BetriebskostenRechnungenListGruppenJahr(ImmutableSortedDictionary<int, ImmutableList<BetriebskostenGruppenRechnungenRechnung>> j)
            {
                Jahre = j;
            }

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

            public BetriebskostenGruppenRechnungenRechnung(BetriebskostenGruppenRechnungenRechnung r)
            {
                // Template for next year (Note: Entity is null here)
                BetreffendesJahr = r.BetreffendesJahr + 1;
                Datum = r.Datum.AddYears(1);
                Betrag = r.Betrag;
                Beschreibung = r.Beschreibung;
                Notiz = r.Notiz;
                Schluessel = r.Schluessel;
                Typ = r.Typ;

                Wohnungen = r.Wohnungen.Select(w => w).ToImmutableList();
                WohnungenIds = r.WohnungenIds.Select(w => w).ToImmutableList();
            }
            public BetriebskostenGruppenRechnungenRechnung(Betriebskostenrechnung r)
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
    }
}
