using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.ViewModels.Rechnungen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenDetailPage : Page
    {
        public ErhaltungsaufwendungenDetailViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int id)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(App.Walter.Erhaltungsaufwendungen.Find(id));
            }
            if (e.Parameter is ErhaltungsaufwendungenDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is null)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel();
            }

            App.ViewModel.Titel.Value = "Erhaltungsaufwendung";
            App.ViewModel.DetailAnhang.Value = new AnhangListViewModel(ViewModel.Entity);

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.ViewModel.RefillCommandContainer(new ICommandBarElement[]
{
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            }, new ICommandBarElement[] { Delete });
            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                if (ViewModel.Id != 0)
                {
                    ViewModel.selfDestruct();
                }
                Frame.GoBack();
            }
        }
    }
}
