using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IPrintViewModel
    {
        string ToString();
        AsyncRelayCommand Print { get; }
        ObservableProperty<int> Jahr { get; }
        IWalterDbService WalterDbService { get; }
    }
}
