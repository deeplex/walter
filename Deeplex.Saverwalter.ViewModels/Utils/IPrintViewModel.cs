using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IPrintViewModel
    {
        string ToString();
        AsyncRelayCommand Print { get; }
        int Jahr { get; set; }
        IWalterDbService WalterDbService { get; }
    }
}
