using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragListPage : Page
    {
        public List<VertragListViewModel> ViewModel { get; set; }

        public ObservableProperty<VertragListViewModel> SelectedVertrag { get; }
            = new ObservableProperty<VertragListViewModel>();

        public VertragListPage()
        {
            InitializeComponent();
            ViewModel = App.Walter.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse)
                .Include(v => v.Mieter).ThenInclude(m => m.Kontakt)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(v => new VertragListViewModel(v))
                .OrderBy(v => v.Beginn.Value).Reverse()
                .ToList();
        }

        private void Vertrag_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VertragDetailViewPage), SelectedVertrag.Value.VertragId,
                new DrillInNavigationTransitionInfo());
        }
    }
}
