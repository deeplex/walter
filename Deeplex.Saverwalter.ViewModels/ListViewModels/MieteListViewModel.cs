using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MieteListViewModel : ListViewModel<MieteListViewModelEntry>
    {
        public ObservableProperty<ImmutableList<MieteListViewModelEntry>> Liste { get; } = new();

        public RelayCommand Add { get; }

        public MieteListViewModel(Vertrag v, INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;
            var self = this;

            Liste.Value = v.Mieten
                .Select(m => new MieteListViewModelEntry(m, self))
                .OrderByDescending(e => e.Zahlungsdatum.Value)
                .ToImmutableList();

            Add = new RelayCommand(_ =>
            {
                var miete = new Miete
                {
                    Vertrag = v,
                    Zahlungsdatum = DateTime.Now.AsUtcKind(),
                    BetreffenderMonat = DateTime.Now.AsUtcKind(),
                    Betrag = Liste.Value.FirstOrDefault()?.Betrag?.Value
                };
                Liste.Value = Liste.Value
                    .Prepend(new MieteListViewModelEntry(miete, this))
                    .ToImmutableList();
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
