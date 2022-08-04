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
        public ObservableProperty<ImmutableList<MietminderungListViewModelEntry>> Liste = new();
        public Guid VertragId;

        public IWalterDbService Db;
        public INotificationService NotificationService;
        public RelayCommand Add { get; }

        public MietMinderungListViewModel(Guid VertragGuid, INotificationService ns, IWalterDbService db)
        {
            VertragId = VertragGuid;
            var self = this;
            NotificationService = ns;
            Db = db;

            Liste.Value = Db.ctx.MietMinderungen
                .Where(m => m.VertragId == VertragGuid)
                .ToList()
                .Select(m => new MietminderungListViewModelEntry(m, self))
                .OrderBy(e => e.Beginn)
                .Reverse()
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
    }
}
