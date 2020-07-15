using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AddresseControl : UserControl
    {
        public AdresseViewModel ViewModel { get; set; }

        public AddresseControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel(App.Walter.Adressen.Find(Id));
            });
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                  "Id",
                  typeof(int),
                  typeof(AddresseControl),
                  new PropertyMetadata(0));
    }
}
