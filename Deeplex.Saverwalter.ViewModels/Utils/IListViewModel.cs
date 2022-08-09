using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IListViewModel
    {
        string ToString();
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
        void SetList();
        RelayCommand Navigate { get; }
        string Filter { get; set; }
    }
}
