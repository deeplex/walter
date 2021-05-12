using Deeplex.Saverwalter.App.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
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
                ViewModel.Zaehler = App.Walter.ZaehlerSet.Find(ZaehlerstandListViewModel.ZaehlerId);
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
            App.Walter.Zaehlerstaende.Add(ViewModel.Entity);
            App.SaveWalter();
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
