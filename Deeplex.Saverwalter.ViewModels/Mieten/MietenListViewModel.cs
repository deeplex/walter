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
        public ObservableProperty<ImmutableList<MietenListViewModelEntry>> Liste { get; } = new();
        public Guid VertragId;

        public IWalterDbService Db;
        public INotificationService NotificationService;
        public RelayCommand Add { get; }

        public MietenListViewModel(Guid VertragGuid, INotificationService ns, IWalterDbService db)
        {
            VertragId = VertragGuid;
            Db = db;
            NotificationService = ns;
            var self = this;

            Liste.Value = Db.ctx.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .ToList()
                .Select(m => new MietenListViewModelEntry(m, self))
                .OrderByDescending(e => e.Zahlungsdatum.Value)
                .ToImmutableList();

            Add = new RelayCommand(_ =>
            {
                var miete = new Miete
                {
                    VertragId = VertragId,
                    Zahlungsdatum = DateTime.Now.AsUtcKind(),
                    BetreffenderMonat = DateTime.Now.AsUtcKind(),
                    Betrag = Liste.Value.FirstOrDefault()?.Betrag?.Value
                };
                Liste.Value = Liste.Value
                    .Prepend(new MietenListViewModelEntry(miete, this))
                    .ToImmutableList();
            }, _ => true);
        }
    }
}
