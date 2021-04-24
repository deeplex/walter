using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModel : BindableBase
    {
        public ObservableProperty<List<BetriebskostenrechnungDetailViewModel>> Liste = new ObservableProperty<List<BetriebskostenrechnungDetailViewModel>>();
        public ImmutableDictionary<BetriebskostenRechungenListWohnungListAdresse, ImmutableList<BetriebskostenRechungenListWohnungListWohnung>> AdresseGroup;
        public List<BetriebskostentypUtil> Betriebskostentypen = Enums.Betriebskostentyp;
        public List<UmlageSchluesselUtil> Betriebskostenschluessel = Enums.UmlageSchluessel;
        public List<HKVO9Util> HKVO9 = Enums.HKVO9;

        private BetriebskostenrechnungDetailViewModel mSelectedRechnung;
        public BetriebskostenrechnungDetailViewModel SelectedRechnung
        {
            get => mSelectedRechnung;
            set
            {
                mSelectedRechnung = value;
                RaisePropertyChangedAuto();
                RaisePropertyChanged(nameof(hasSelectedKontakt));
            }
        }
        public bool hasSelectedKontakt => SelectedRechnung != null;

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenListViewModel()
        {
            Liste.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Include(b => b.Allgemeinzaehler)
                .Select(w => new BetriebskostenrechnungDetailViewModel(w))
                .ToList();

            var Gruppen = App.Walter.Betriebskostenrechnungsgruppen.ToList()
                .GroupBy(p => new SortedSet<int>(p.Rechnung.Gruppen.Select(gr => gr.WohnungId)), new SortedSetIntEqualityComparer())
                .Select(g => new BetriebskostenRechnungenBetriebskostenGruppe(g.Key))
                .ToList();

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
