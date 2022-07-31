using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : ListViewModel<WohnungListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Wohnungen";

        protected override ImmutableList<WohnungListViewModelEntry> updateList(string filter)
            => List.Value.Where(v => applyFilter(filter, v.Bezeichnung, v.Anschrift)).ToImmutableList();

        public WohnungListViewModel(IWalterDbService db, INotificationService ns): this(ns)
        {
            AllRelevant = transform(db, include(db));
            List.Value = AllRelevant;
        }

        public WohnungListViewModel(IWalterDbService db, INotificationService ns, Adresse a): this(ns)
        {
            AllRelevant = transform(db, include(db).Where(w => w.AdresseId == a.AdresseId).ToList());
            List.Value = AllRelevant;
        }

        public WohnungListViewModel(IWalterDbService db, INotificationService ns, Umlage r) : this(ns)
        {
            AllRelevant = transform(db, include(db).Where(w => w.Umlagen.Exists(b => b.UmlageId == r.UmlageId)).ToList());
            List.Value = AllRelevant;
        }


        public WohnungListViewModel(IWalterDbService db, INotificationService ns, Betriebskostenrechnung r): this(ns)
        {
            AllRelevant = transform(db,
                include(db)
                    .Where(w => w.Betriebskostenrechnungen
                    .Exists(b => b.BetriebskostenrechnungId == r.BetriebskostenrechnungId))
                    .ToList());
            List.Value = AllRelevant;
        }

        public WohnungListViewModel(IWalterDbService db, INotificationService ns, IPerson p)
        {
            var vertragIds = db.ctx.MieterSet
                .ToList()
                .Where(m => m.PersonId == p.PersonId)
                .Select(m => m.VertragId)
                .ToList();

            var wohnungen = db.ctx.Vertraege
                .ToList()
                .Where(v =>
                    vertragIds.Exists(i => v.VertragId == i) ||
                    v.Wohnung.BesitzerId == p.PersonId ||
                    v.AnsprechpartnerId == p.PersonId)
                .Select(v => v.Wohnung)
                .ToList();

            AllRelevant = transform(db, include(db)
                .Where(w => wohnungen.Exists(e => e.WohnungId == w.WohnungId))
                .ToList());

            List.Value = AllRelevant;
        }

        private List<Wohnung> include(IWalterDbService db)
        {
            return db.ctx.Wohnungen
               .Include(w => w.Anhaenge)
               .Include(w => w.Adresse)
               .ThenInclude(a => a.Anhaenge)
               .ToList();
        }

        private ImmutableList<WohnungListViewModelEntry> transform(IWalterDbService db, List<Wohnung> list)
        {
            return list
                .Select(w => new WohnungListViewModelEntry(w, db))
                .ToImmutableList();
        }

        private WohnungListViewModel(INotificationService ns)
        {
            Navigate = new RelayCommand(el => ns.Navigation((Wohnung)el), _ => true);
        }
    }
}
