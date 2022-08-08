using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IListViewModel<IListViewModelEntry>
    {
        string ToString();
        IEnumerable<IListViewModelEntry> AllRelevant { get; set; }
        ObservableProperty<ImmutableList<IListViewModelEntry>> List { get; set; }
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
        void SetList();
        RelayCommand Navigate { get; }
        string Filter { get; set; }
    }
}
