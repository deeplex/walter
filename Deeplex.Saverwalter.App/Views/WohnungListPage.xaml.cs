using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class WohnungListPage : Page
    {
        public ViewModels.WohnungListViewModel ViewModel { get; set; }

        public WohnungListPage()
        {
            ViewModel = new ViewModels.WohnungListViewModel();
            InitializeComponent();
        }
    }
}
