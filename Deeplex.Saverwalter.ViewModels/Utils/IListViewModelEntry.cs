using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class ListViewModelEntry<T>: BindableBase
    {
        public abstract override string ToString();
        T Entity { get; }
    }
}
