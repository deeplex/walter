using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenRechnungenListViewModel : ListViewModel<BetriebskostenRechnungenListEntry>, IListViewModel
    {
        public override string ToString() => "Betriebskostenrechnungen";

        protected override ImmutableList<BetriebskostenRechnungenListEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Typ.ToDescriptionString(), v.ToString(), v.BetreffendesJahr.ToString())).ToImmutableList();

        public BetriebskostenRechnungenListViewModel(IWalterDbService db, INotificationService ns): this(ns)
        {
            AllRelevant = transform(db, include(db));
            List.Value = AllRelevant;
        }

        public BetriebskostenRechnungenListViewModel(IWalterDbService db, INotificationService ns, Vertrag v): this(ns)
        {
            AllRelevant = transform(db,
                include(db)
                    .Where(b => b.Umlage.Wohnungen.Exists(w => v.Wohnung.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant;
        }

        public BetriebskostenRechnungenListViewModel(IWalterDbService db, INotificationService ns, Wohnung w): this(ns)
        {
            AllRelevant = transform(db,
                include(db)
                    .Where(b => b.Umlage.Wohnungen.Exists(i => i.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant;
        }

        private List<Betriebskostenrechnung> include(IWalterDbService db)
        {
            return db.ctx.Betriebskostenrechnungen
                .Include(b => b.Anhaenge)
                .Include(b => b.Umlage).ThenInclude(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(b => b.Umlage).ThenInclude(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Wohnungen).ThenInclude(w => w.Anhaenge)
                .Include(b => b.Umlage.Zaehler).ThenInclude(b => b.Anhaenge)
                .Include(b => b.Umlage.Wohnungen).ThenInclude(g => g.Adresse).ThenInclude(a => a.Anhaenge)
                .ToList();
        }
        private ImmutableList<BetriebskostenRechnungenListEntry> transform(IWalterDbService db, List<Betriebskostenrechnung> list)
        {
            return list
                .Select(w => new BetriebskostenRechnungenListEntry(w))
                .ToImmutableList();
        }

        private BetriebskostenRechnungenListViewModel(INotificationService ns)
        {
            Navigate = new RelayCommand(el => ns.Navigation((Betriebskostenrechnung)el), _ => true);
        }
    }
}
