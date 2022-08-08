using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IDetailViewModel<T>
    {
        string ToString();
        void checkForChanges();
        RelayCommand Save { get; }
        AsyncRelayCommand Delete { get; }
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
        T Entity { get; }
        void SetEntity(T e);
    }
}
