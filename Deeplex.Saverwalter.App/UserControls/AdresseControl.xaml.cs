using Deeplex.Saverwalter.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class AdresseControl : UserControl
    {
        public int AdressId
        {
            get { return (int)GetValue(AdressIdProperty); }
            set { SetValue(AdressIdProperty, value); }
        }   

        public static readonly DependencyProperty AdressIdProperty =
           DependencyProperty.Register("AdressId", typeof(int), typeof(AdresseControl), new PropertyMetadata(0));
        
        AdresseViewModel ViewModel { get; set; }

        public AdresseControl()
        {
            Loaded += (args, sender) =>
            {
                if (AdressId > 0)
                {
                    ViewModel = new AdresseViewModel(AdressId);
                }
                InitializeComponent();
            };
        }
    }
}
