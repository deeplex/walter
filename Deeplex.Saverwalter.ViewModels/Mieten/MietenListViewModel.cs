using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
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

        public IWalterDbService Db;
        public INotificationService NotificationService;
        public RelayCommand Add { get; }

        public MietenListViewModel(Guid VertragGuid, INotificationService ns, IWalterDbService db)
        {
            VertragId = VertragGuid;
            Db = db;
            var self = this;
            Liste.Value = Db.ctx.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .ToList()
                .Select(m => new MietenListViewModelEntry(m, self))
                .OrderBy(e => e.Zahlungsdatum)
                .Reverse()
                .ToImmutableList();

            Add = new RelayCommand(_ =>
            {
                var miete = new Miete
                {
                    VertragId = VertragId,
                    BetreffenderMonat = DateTime.Now.AsUtcKind(),
                    Betrag = Liste.Value.FirstOrDefault()?.Betrag.Value
                };
                Liste.Value = Liste.Value
                    .Prepend(new MietenListViewModelEntry(miete, this))
                    .ToImmutableList();
            }, _ => true);
        }
    }
}
