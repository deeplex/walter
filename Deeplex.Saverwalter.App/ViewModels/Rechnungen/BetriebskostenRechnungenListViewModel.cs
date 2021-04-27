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
        public ObservableProperty<List<BetriebskostenRechnungenListEntry>> Liste = new ObservableProperty<List<BetriebskostenRechnungenListEntry>>();

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
            Liste.Value = App.Walter.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Include(b => b.Zaehler)
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToList();
        }
    }

    public sealed class BetriebskostenRechnungenListEntry
    {
        private readonly Betriebskostenrechnung Entity;
        public int Id => Entity.BetriebskostenrechnungId;
        public List<Wohnung> Wohnungen { get; }
        public string TypString => Entity.Typ.ToString();
        public string BetreffendesJahrString => Entity.BetreffendesJahr.ToString();
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
