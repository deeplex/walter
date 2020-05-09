using Deeplex.Saverwalter.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktDetailPage : Page
    {
        public KontaktDetailViewModel ViewModel { get; set; }

        public KontaktDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int kontaktId)
            {
                ViewModel = new KontaktDetailViewModel(kontaktId);
            }
            else // If invoked using "Add"
            {
                //ViewModel = new KontaktViewModel
                //{
                //    IsNewCustomer = true,
                //    IsInEdit = true
                //};
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }
    }
}
