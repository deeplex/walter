using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModel : ListViewModel<ErhaltungsaufwendungenListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Erhaltungsaufwendungen";

        protected override ImmutableList<ErhaltungsaufwendungenListViewModelEntry> updateList(string filter)
            => AllRelevant.Where(v => applyFilter(filter, v.Wohnung.Anschrift, v.Bezeichnung)).ToImmutableList();

        public ErhaltungsaufwendungenListViewModel(IWalterDbService db, INotificationService ns): this(ns)
        {
            AllRelevant = transform(db, include(db));
            List.Value = AllRelevant;
        }

        public ErhaltungsaufwendungenListViewModel(IWalterDbService db, INotificationService ns, Vertrag v) : this(db, ns, v.Wohnung) { }

        public ErhaltungsaufwendungenListViewModel(IWalterDbService db, INotificationService ns, Wohnung w) : this(ns)
        {
            AllRelevant = transform(db,
                include(db)
                    .Where(e => w.Erhaltungsaufwendungen
                    .Exists(i => i.ErhaltungsaufwendungId == e.ErhaltungsaufwendungId))
                    .ToList());
            List.Value = AllRelevant;
        }

        private List<Erhaltungsaufwendung> include(IWalterDbService db)
        {
            return db.ctx.Erhaltungsaufwendungen
                .Include(e => e.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(w => w.Anhaenge)
                .ToList();
        }
        private ImmutableList<ErhaltungsaufwendungenListViewModelEntry> transform(IWalterDbService db, List<Erhaltungsaufwendung> list)
        {
            return list.Select(w => new ErhaltungsaufwendungenListViewModelEntry(w, db)).ToImmutableList();
        }

        private ErhaltungsaufwendungenListViewModel(INotificationService ns)
        {
            Navigate = new RelayCommand(el => ns.Navigation((Erhaltungsaufwendung)el), _ => true);

        }
    }
}
