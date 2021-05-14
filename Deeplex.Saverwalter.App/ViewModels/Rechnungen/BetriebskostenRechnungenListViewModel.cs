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
    public sealed class BetriebskostenRechnungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>> Liste =
            new ObservableProperty<ImmutableList<BetriebskostenRechnungenListEntry>>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<BetriebskostenRechnungenListEntry> AllRelevant { get; set; }

        private BetriebskostenRechnungenListEntry mSelectedRechnung;
        public BetriebskostenRechnungenListEntry SelectedRechnung
        {
            get => mSelectedRechnung;
            set
            {
                mSelectedRechnung = value;
                RaisePropertyChangedAuto();
            }
        }

        public TreeViewNode AddBetriebskostenTree;
        public BetriebskostenRechnungenListViewModel()
        {
            AllRelevant = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ThenInclude(a => a.Wohnungen)
                .Include(b => b.Zaehler)
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToImmutableList();
            Liste.Value = AllRelevant;
        }
    }

    public sealed class BetriebskostenRechnungenListEntry
    {
        public readonly Betriebskostenrechnung Entity;
        public int Id => Entity.BetriebskostenrechnungId;
        public string Beschreibung => Entity.Beschreibung;
        public List<Wohnung> Wohnungen { get; }
        public string TypString => Entity.Typ.ToDescriptionString();
        public int BetreffendesJahr => Entity.BetreffendesJahr;
        public string BetreffendesJahrString => BetreffendesJahr.ToString();
        public string BetragString => Entity.Betrag.ToString() + "€";
        public string AdressenBezeichnung { get; }

        public BetriebskostenRechnungenListEntry(Betriebskostenrechnung r)
        {
            Entity = r;
            Wohnungen = r.Gruppen.Select(g => g.Wohnung).ToList();
            AdressenBezeichnung = string.Join(" — ", App.Walter.Wohnungen
            .Include(w => w.Adresse)
            .Where(w => Wohnungen.Contains(w))
            .ToList()
            .GroupBy(w => w.Adresse)
            .ToDictionary(g => g.Key, g => g.ToList())
            .Select(adr =>
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
}
