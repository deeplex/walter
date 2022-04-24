using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietMinderungListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietminderungListViewModelEntry>> Liste
            = new ObservableProperty<ImmutableList<MietminderungListViewModelEntry>>();
        public Guid VertragId;

        public IAppImplementation Impl;
        public IWalterDbService Db;

        public MietMinderungListViewModel(Guid VertragGuid, IAppImplementation impl, IWalterDbService db)
        {
            VertragId = VertragGuid;
            var self = this;
            Impl = impl;
            Db = db;

            Liste.Value = Db.ctx.MietMinderungen
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietminderungListViewModelEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(MietMinderung z)
        {
            Liste.Value = Liste.Value.Add(new MietminderungListViewModelEntry(z, this));
        }
    }
}
