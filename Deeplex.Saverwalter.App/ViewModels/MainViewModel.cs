using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MainViewModel
    {
        public ObservableProperty<ImmutableList<AnhangViewModel>> Anhang = new ObservableProperty<ImmutableList<AnhangViewModel>>();
        public MainViewModel()
        {
            Anhang.Value = App.Walter.Anhaenge.Select(a => new AnhangViewModel(a)).ToImmutableList();
        }
    }

    public sealed class AnhangViewModel : BindableBase
    {
        public Anhang Entity { get; }

        public AnhangViewModel(Anhang a)
        {
            Entity = a;
        }

        public string DateiName => Entity.FileName;
    }
}
