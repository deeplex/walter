using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3
{
    public partial class DetailViewPage<T> : Page
    {
        public DetailViewModel<T> ViewModel = App.Container.GetInstance<DetailViewModel<T>>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.Window.CommandBar.MainContent = new DetailCommandBarControl() { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
