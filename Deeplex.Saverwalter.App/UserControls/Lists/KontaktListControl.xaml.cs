using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.Utils.Elements;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class KontaktListControl : UserControl
    {
        public KontaktListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Kontakte.Value = ViewModel.AllRelevant;
            if (Filter != "")
            {
                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value.Where(v =>
                    applyFilter(Filter, v.Anschrift, v.Name, v.Vorname, v.Email, v.Telefon))
                    .ToImmutableList();
            }
        }

        public KontaktListControl()
        {
            InitializeComponent();
            ViewModel = new KontaktListViewModel();
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(VertragListControl),
                  new PropertyMetadata(""));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var sk = ViewModel.SelectedKontakt;
            if (sk != null)
            {
                var target =
                    sk.Type == typeof(NatuerlichePerson) ? typeof(NatuerlichePersonDetailPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailPage) :
                    null;

                App.ViewModel.Navigate(target, App.Walter.FindPerson(sk.Guid));
            }
        }

        private void DataGrid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ViewModel.SelectedKontakt = (e.OriginalSource as FrameworkElement).DataContext as KontaktListEntry;
        }
    }
}
