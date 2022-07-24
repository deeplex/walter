using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IListViewModel
    {
        string ToString();
        RelayCommand Add { get; }
        string Filter { get; set; }
    }
}
