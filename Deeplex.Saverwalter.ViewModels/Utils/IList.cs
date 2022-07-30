using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IListViewModel
    {
        string ToString();
        RelayCommand Navigate { get; }
        string Filter { get; set; }
    }
}
