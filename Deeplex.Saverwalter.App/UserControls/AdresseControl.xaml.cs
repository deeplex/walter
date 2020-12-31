using Deeplex.Saverwalter.App.ViewModels;
using Deeplex.Saverwalter.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AdresseControl : UserControl
    {
        public AdresseViewModel ViewModel { get; set; }

        public AdresseControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(IdProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel(App.Walter.Adressen.Find(Id));
            });

            RegisterPropertyChangedCallback(AdresseProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<IAdresse>(Adresse);
            });
        }

        public IAdresse Adresse
        {
            get { return (IAdresse)GetValue(AdresseProperty); }
            set { SetValue(AdresseProperty, value); }
        }

        public static readonly DependencyProperty AdresseProperty
            = DependencyProperty.Register(
                "value",
                typeof(IAdresse),
                typeof(AdresseControl),
                new PropertyMetadata(null));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register(
                  "Id",
                  typeof(int),
                  typeof(AdresseControl),
                  new PropertyMetadata(0));
    }
}
