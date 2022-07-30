using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IPrint
    {
        string ToString();
        AsyncRelayCommand Print { get; }
        ObservableProperty<int> Jahr { get; }
    }
}
