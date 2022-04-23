using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietenListViewModelEntry>> Liste = new();
        public Guid VertragId;

        public AppViewModel Avm;
        public IAppImplementation Impl;

        public MietenListViewModel(Guid VertragGuid, IAppImplementation impl, AppViewModel avm)
        {
            VertragId = VertragGuid;
            Avm = avm;
            Impl = impl;
            var self = this;
            Liste.Value = Avm.ctx.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietenListViewModelEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(Miete z)
        {
            Liste.Value = Liste.Value.Add(new MietenListViewModelEntry(z, this));
        }
    }
}
