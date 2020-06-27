using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MainViewModel
    {
        public AnhangViewModel Anhang = new AnhangViewModel();
        public ObservableProperty<string> Titel = new ObservableProperty<string>();
        public MainViewModel()
        {
            Titel.Value = "Walter";
        }
    }
}
