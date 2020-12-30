using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
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

            RegisterPropertyChangedCallback(WohnungProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<Wohnung>(Wohnung);
            });

            RegisterPropertyChangedCallback(NPersonProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<NatuerlichePerson>(NPerson);
            });

            RegisterPropertyChangedCallback(JPersonProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<JuristischePerson>(JPerson);
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

        public Wohnung Wohnung
        {
            get { return (Wohnung)GetValue(WohnungProperty); }
            set { SetValue(WohnungProperty, value); }
        }

        public static readonly DependencyProperty WohnungProperty
            = DependencyProperty.Register(
                  "Wohnung",
                  typeof(Wohnung),
                  typeof(AddresseControl),
                  new PropertyMetadata(null));

        public NatuerlichePerson NPerson
        {
            get { return (NatuerlichePerson)GetValue(NPersonProperty); }
            set { SetValue(NPersonProperty, value); }
        }

        public static readonly DependencyProperty NPersonProperty
            = DependencyProperty.Register(
                  "NPerson",
                  typeof(NatuerlichePerson),
                  typeof(AddresseControl),
                  new PropertyMetadata(null));

        public JuristischePerson JPerson
        {
            get { return (JuristischePerson)GetValue(JPersonProperty); }
            set { SetValue(JPersonProperty, value); }
        }

        public static readonly DependencyProperty JPersonProperty
            = DependencyProperty.Register(
                  "JPerson",
                  typeof(JuristischePerson),
                  typeof(AddresseControl),
                  new PropertyMetadata(null));
    }
}
