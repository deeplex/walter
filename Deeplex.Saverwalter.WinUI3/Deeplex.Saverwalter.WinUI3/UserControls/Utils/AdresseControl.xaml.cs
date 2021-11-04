using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class AdresseControl : UserControl
    {
        public AdresseViewModel ViewModel { get; set; }

        public AdresseControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(AdresseProperty, (idDepObject, idProp) =>
            {
                ViewModel = new AdresseViewModel<IAdresse>(Adresse, App.ViewModel);
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

        private void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.updateAdressen(Strasse.Text.Trim(), Hausnr.Text.Trim(), Postleitzahl.Text.Trim(), Stadt.Text.Trim());
        }

        private void QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            updateAdresse(sender, args.QueryText);
        }

        private void lostFocus(object sender, RoutedEventArgs e)
        {
            updateAdresse((AutoSuggestBox)sender, ((TextBox)e.OriginalSource).Text);
        }

        private void updateAdresse(AutoSuggestBox sender, string text)
        {
            if (sender == Strasse)
            {
                ViewModel.Strasse = text;
            }
            if (sender == Hausnr)
            {
                ViewModel.Hausnummer = text;
            }
            if (sender == Postleitzahl)
            {
                ViewModel.Postleitzahl = text;
            }
            if (sender == Stadt)
            {
                ViewModel.Stadt = text;
            }
        }
    }
}
