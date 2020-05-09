using Deeplex.Saverwalter.App.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragListPage : Page
    {
        public List<VertragListViewModel> ViewModel { get; set; }

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
    }
}
