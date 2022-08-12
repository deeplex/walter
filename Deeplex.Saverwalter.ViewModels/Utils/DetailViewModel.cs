using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IDetailViewModel
    {
        string ToString();
        void checkForChanges();
        RelayCommand Save { get; }
        AsyncRelayCommand Delete { get; }
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
    }

    public abstract class DetailViewModel<T> : BindableBase, IDetailViewModel
    {
        T Entity { get; }
        public abstract void SetEntity(T e);
        public abstract override string ToString();
        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; protected set; }
        public IWalterDbService WalterDbService { get; protected set; }
        public INotificationService NotificationService { get; protected set; }
        public abstract void checkForChanges();
    }
}
