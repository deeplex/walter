using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Deeplex.Saverwalter.Model;
using Microsoft.Toolkit.Uwp.UI.Controls;
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

            if (Kontakte != null)
            {
                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value.Where(v =>
                    Kontakte.Any(k => k.Guid == v.Guid))
                    .ToImmutableList();
            }
        }

        public KontaktListControl()
        {
            InitializeComponent();
            ViewModel = new KontaktListViewModel();
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(KontakteProperty, (DepObj, Prop) => UpdateFilter());
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

        public ImmutableList<KontaktListEntry> Kontakte
        {
            get { return (ImmutableList<KontaktListEntry>)GetValue(KontakteProperty); }
            set { SetValue(KontakteProperty, value); }
        }

        public static readonly DependencyProperty KontakteProperty
            = DependencyProperty.Register(
                  "KontakteProperty",
                  typeof(ImmutableList<KontaktListEntry>),
                  typeof(VertragListControl),
                  new PropertyMetadata(null));

        public bool VertragBool => VertragGuid != Guid.Empty;
        public Guid VertragGuid
        {
            get { return (Guid)GetValue(VertragGuidProperty); }
            set { SetValue(VertragGuidProperty, value); }
        }

        public static readonly DependencyProperty VertragGuidProperty
            = DependencyProperty.Register(
                  "VertragGuid",
                  typeof(Guid),
                  typeof(VertragListControl),
                  new PropertyMetadata(Guid.Empty));

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

        private async void RemoveMieter_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                var guid = ((KontaktListEntry)((Button)sender).DataContext).Guid;

                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value
                    .Where(k => guid != k.Guid).ToImmutableList();
                App.Walter.MieterSet
                    .Where(m => m.PersonId == guid && m.VertragId == VertragGuid)
                    .ToList().ForEach(m => App.Walter.MieterSet.Remove(m));
                App.SaveWalter();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((KontaktListEntry)((DataGrid)sender).SelectedItem)?.Entity;
            if (a is NatuerlichePerson n)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(n);
            }
            else if (a is JuristischePerson j)
            {
                App.ViewModel.ListAnhang.Value = new AnhangListViewModel(j);
            }
        }
    }
}
