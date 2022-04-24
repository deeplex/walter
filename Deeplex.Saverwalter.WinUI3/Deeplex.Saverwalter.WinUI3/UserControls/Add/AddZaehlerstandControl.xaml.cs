using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class AddZaehlerstandControl : UserControl
    {
        public ZaehlerstandDetailViewModel ViewModel = new ZaehlerstandDetailViewModel();
        public DateTimeOffset Datum
        {
            get => ViewModel.Datum;
            set
            {
                ViewModel.Datum = value.DateTime;
            }
        }

        public AddZaehlerstandControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(ZaehlerstandListViewModelProperty, (ZaehlerIdDepObject, ZaehlerIdProp) =>
            {
                ViewModel.Zaehler = App.WalterService.ctx.ZaehlerSet.Find(ZaehlerstandListViewModel.ZaehlerId);
                var Last = ViewModel.Zaehler?.Staende.OrderBy(e => e.Datum).LastOrDefault();
                if (Last == null)
                {
                    ViewModel.Stand = 0;
                }
                else
                {
                    ViewModel.Stand = Last.Stand;
                }
                ViewModel.Datum = DateTime.Today;
            });
        }

        private void AddZaehlerstand_Click(object sender, RoutedEventArgs e)
        {
            App.WalterService.ctx.Zaehlerstaende.Add(ViewModel.Entity);
            App.WalterService.SaveWalter();
            ZaehlerstandListViewModel.AddToList(ViewModel.Entity);
            ViewModel = new ZaehlerstandDetailViewModel()
            {
                Zaehler = ViewModel.Zaehler,
            };
            if (AddZaehlerButton.Flyout is Flyout f)
            {
                f.Hide();
            }
        }

        public ZaehlerstandListViewModel ZaehlerstandListViewModel
        {
            get { return (ZaehlerstandListViewModel)GetValue(ZaehlerstandListViewModelProperty); }
            set { SetValue(ZaehlerstandListViewModelProperty, value); }
        }

        public static readonly DependencyProperty ZaehlerstandListViewModelProperty
            = DependencyProperty.Register(
            "ZaehlerstandListViewModel",
            typeof(ZaehlerstandListViewModel),
            typeof(ZaehlerstandListControl),
            new PropertyMetadata(null));
    }
}
