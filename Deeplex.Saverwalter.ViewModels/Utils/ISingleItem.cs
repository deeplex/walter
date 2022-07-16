using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface ISingleItem
    {
        string ToString();
        void checkForChanges();
        RelayCommand Save { get; }
        AsyncRelayCommand Delete { get; }
    }
}
