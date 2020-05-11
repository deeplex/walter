using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class AdresseControl : UserControl
    {
        public int AdressId
        {
            get => (int)GetValue(AdressIdProperty);
            set { SetValue(AdressIdProperty, value); }
        }
        public static readonly DependencyProperty AdressIdProperty =
           DependencyProperty.Register("AdressId", typeof(int), typeof(AdresseControl), new PropertyMetadata(0));

        AdresseControlModel ViewModel { get; set; }

        public AdresseControl()
        {
            Loaded += (args, sender) =>
            {
                if (AdressId > 0)
                {
                    ViewModel = new AdresseControlModel(AdressId);
                }
                InitializeComponent();
            };
        }
    }
}
