using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktListPage : Page
    {
        public ViewModels.KontaktListViewModel ViewModel { get; set; }

        public KontaktListPage()
        {
            ViewModel = new ViewModels.KontaktListViewModel(MainPage.Walter);
            InitializeComponent();
        }

    }
}
