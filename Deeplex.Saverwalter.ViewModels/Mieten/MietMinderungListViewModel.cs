using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietMinderungListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietMinderungListEntry>> Liste
            = new ObservableProperty<ImmutableList<MietMinderungListEntry>>();
        public Guid VertragId;

        public IAppImplementation Impl;
        public AppViewModel Avm;

        public MietMinderungListViewModel(Guid VertragGuid, IAppImplementation impl, AppViewModel avm)
        {
            VertragId = VertragGuid;
            var self = this;
            Impl = impl;
            Avm = avm;

            Liste.Value = Avm.ctx.MietMinderungen
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietMinderungListEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(MietMinderung z)
        {
            Liste.Value = Liste.Value.Add(new MietMinderungListEntry(z, this));
        }
    }

    public sealed class MietMinderungListEntry
    {
        public MietMinderung Entity { get; }

        public string BeginnString => Entity.Beginn.ToString("dd.MM.yyyy");
        public string EndeString => Entity.Ende is DateTime d ? d.ToString("dd.MM.yyyy") : "Offen";
        public string MinderungString => Entity.Minderung.ToString();
        public string Notiz => Entity.Notiz;

        public MietMinderungListEntry(MietMinderung m, MietMinderungListViewModel vm)
        {
            Entity = m;

            SelfDestruct = new AsyncRelayCommand(async _ =>
            {
                if (await vm.Impl.Confirmation())
                {
                    vm.Liste.Value = vm.Liste.Value.Remove(this);
                    vm.Avm.ctx.MietMinderungen.Remove(Entity);
                    vm.Avm.SaveWalter();
                }

            }, _ => true);
        }
        public AsyncRelayCommand SelfDestruct;
    }
}
