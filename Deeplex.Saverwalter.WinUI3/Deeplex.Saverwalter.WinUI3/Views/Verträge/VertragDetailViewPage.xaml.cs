using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; set; }

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Guid vertragId)
            {
                ViewModel = new VertragDetailViewModel(vertragId, App.Impl, App.WalterService);
            }
            else if (e.Parameter is VertragDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else
            {
                ViewModel = new VertragDetailViewModel(App.Impl, App.WalterService);
            }

            App.Window.CommandBar.MainContent = new UserControls.VertragCommandBarControl() { ViewModel = ViewModel };
            // TODO
            //App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            base.OnNavigatedTo(e);
        }
    }
}
