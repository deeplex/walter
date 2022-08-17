using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietminderungListViewModel : ListViewModel<MietminderungListViewModelEntry>
    {
        public ObservableProperty<ImmutableList<MietminderungListViewModelEntry>> Liste = new();

        public RelayCommand Add { get; }

        public MietminderungListViewModel(Vertrag v, INotificationService ns, IWalterDbService db)
        {
            var self = this;
            NotificationService = ns;
            WalterDbService = db;

            Liste.Value = WalterDbService.ctx.MietMinderungen
                .Where(m => m.Vertrag.VertragId == v.VertragId)
                .ToList()
                .Select(m => new MietminderungListViewModelEntry(m, self))
                .OrderByDescending(e => e.Beginn.Value)
                .ToImmutableList();

            Add = new RelayCommand(_ =>
            {
                var mm = new Mietminderung
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
