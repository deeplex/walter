using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class AdresseControl : UserControl
    {
        public AdresseViewModel ViewModel { get; set; }

        public AdresseControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(AdresseProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<IAdresse>(
                    Adresse,
                    App.Container.GetInstance<IWalterDbService>(),
                    App.Container.GetInstance<INotificationService>());
            });
        }

        public IAdresse Adresse
        {
            get { return (IAdresse)GetValue(AdresseProperty); }
            set { SetValue(AdresseProperty, value); }
        }

        public static readonly DependencyProperty AdresseProperty
            = DependencyProperty.Register(
                "Adresse",
                typeof(IAdresse),
                typeof(AdresseControl),
                new PropertyMetadata(null));
    }
}
