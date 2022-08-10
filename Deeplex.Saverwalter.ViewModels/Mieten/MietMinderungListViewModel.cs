using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietMinderungListViewModel : ListViewModel<MietminderungListViewModelEntry>
    {
        public ObservableProperty<ImmutableList<MietminderungListViewModelEntry>> Liste = new();
        public Guid VertragId;

        public RelayCommand Add { get; }

        public MietMinderungListViewModel(Guid VertragGuid, INotificationService ns, IWalterDbService db)
        {
            VertragId = VertragGuid;
            var self = this;
            NotificationService = ns;
            WalterDbService = db;

            Liste.Value = WalterDbService.ctx.MietMinderungen
                .Where(m => m.VertragId == VertragGuid)
                .ToList()
                .Select(m => new MietminderungListViewModelEntry(m, self))
                .OrderByDescending(e => e.Beginn.Value)
                .ToImmutableList();

            Add = new RelayCommand(_ =>
            {
                var mm = new MietMinderung
                {
                    Beginn = DateTime.Today.AsUtcKind(),
                    Ende = DateTime.Today.AsUtcKind().AddDays(30),
                    Minderung = 0.1,
                };
                Liste.Value = Liste.Value.Prepend(new MietminderungListViewModelEntry(mm, this)).ToImmutableList();
            }, _ => true);
        }

        protected override void updateList()
        {
            throw new NotImplementedException();
        }

        public override void SetList()
        {
            throw new NotImplementedException();
        }
    }
}
