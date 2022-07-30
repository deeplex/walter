using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.Services
{
    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }
}
