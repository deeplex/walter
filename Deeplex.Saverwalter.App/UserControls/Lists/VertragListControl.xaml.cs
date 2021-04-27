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

        public VertragListControl()
        {
            InitializeComponent();
            ViewModel = new VertragListViewModel();

            RegisterPropertyChangedCallback(WohnungIdProperty, (WohnungIdDepObject, WohnungIdProp) =>
            {
                ViewModel.Vertraege = ViewModel.Vertraege.Where(v => v.Wohnung.WohnungId == WohnungId).ToImmutableList();
            });

            RegisterPropertyChangedCallback(PersonIdProperty, (PersonIddepObject, PersonIdProp) =>
            {
                ViewModel.Vertraege = ViewModel.Vertraege.Where(v =>
                    v.Wohnung.BesitzerId == PersonId ||
                    v.Mieter.Contains(PersonId))
                    .ToImmutableList();
            });
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
    }
}
