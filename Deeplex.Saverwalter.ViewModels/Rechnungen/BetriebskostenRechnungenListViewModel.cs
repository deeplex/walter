using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
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

        public BetriebskostenRechnungenListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.Betriebskostenrechnungen
                .Include(b => b.Gruppen)
                .ThenInclude(g => g.Wohnung)
                .ThenInclude(w => w.Adresse)
                .ThenInclude(a => a.Wohnungen)
                .Include(b => b.Zaehler)
                .Select(w => new BetriebskostenRechnungenListEntry(w, avm))
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
        public Betriebskostentyp Typ => Entity.Typ;
        public int BetreffendesJahr => Entity.BetreffendesJahr;
        public double Betrag => Entity.Betrag;
        public string AdressenBezeichnung { get; }

        public BetriebskostenRechnungenListEntry(Betriebskostenrechnung r, AppViewModel avm)
        {
            Entity = r;
            Wohnungen = r.Gruppen.Select(g => g.Wohnung).ToList();
            AdressenBezeichnung = Entity.GetWohnungenBezeichnung(avm);
        }
    }
}
