using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModel : ListViewModel<BetriebskostenRechnungenListEntry>, IListViewModel
    {
        public override string ToString() => "Betriebskostenrechnungen";

        protected override ImmutableList<BetriebskostenRechnungenListEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Typ.ToDescriptionString(), v.AdressenBezeichnung, v.BetreffendesJahr.ToString())).ToImmutableList();

        public BetriebskostenRechnungenListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Betriebskostenrechnungen
                .Include(b => b.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Wohnungen).ThenInclude(w => w.Anhaenge)
                .Include(b => b.Zaehler).ThenInclude(b => b.Anhaenge)
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToImmutableList();
            List.Value = AllRelevant;
        }
    }
}
