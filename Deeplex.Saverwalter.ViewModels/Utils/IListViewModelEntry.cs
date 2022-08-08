using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IListViewModelEntry<T>
    {
        string ToString();
        T Entity { get; }
        void checkForChanges();
    }
}
