using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.App.Views;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class VertragListControl : UserControl
    {
        public VertragListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Vertraege.Value = ViewModel.AllRelevant;
            if (Filter != "")
            {
                bool low(string str) => str.ToLower().Contains(Filter.ToLower());
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v =>
                    low(v.Wohnung.Bezeichnung) || low(v.AuflistungMieter) || low(v.Anschrift))
                    .ToImmutableList();
            }
            if (PersonId != Guid.Empty)
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v =>
                    v.Wohnung.BesitzerId == PersonId ||
                    v.Mieter.Contains(PersonId))
                    .ToImmutableList();
            }
            if (WohnungId != 0)
            {
                ViewModel.Vertraege.Value = ViewModel.Vertraege.Value.Where(v => v.Wohnung.WohnungId == WohnungId).ToImmutableList();
            }
        }

        public VertragListControl()
        {
            InitializeComponent();
            ViewModel = new VertragListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(PersonIdProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
        }

        public Guid PersonId
        {
            get { return (Guid)GetValue(PersonIdProperty); }
            set { SetValue(PersonIdProperty, value); }
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedVertrag.Value != null)
            {
                App.ViewModel.Navigate(typeof(VertragDetailViewPage), ViewModel.SelectedVertrag.Value.VertragId);
            }
        }

        public static readonly DependencyProperty PersonIdProperty
            = DependencyProperty.Register(
                "PersonId",
                typeof(Guid),
                typeof(VertragListControl),
                new PropertyMetadata(Guid.Empty));

        public int WohnungId
        {
            get { return (int)GetValue(WohnungIdProperty); }
            set { SetValue(WohnungIdProperty, value); }
        }

        public static readonly DependencyProperty WohnungIdProperty
            = DependencyProperty.Register(
                  "WohnungId",
                  typeof(int),
                  typeof(VertragListControl),
                  new PropertyMetadata(0));

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
    }
}
