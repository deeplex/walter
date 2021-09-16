using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.WinUI3.Views
{
    public sealed partial class KontaktListPage : Page
    {
        public KontaktListViewModel ViewModel = new KontaktListViewModel(App.ViewModel);

        public KontaktListPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Kontakte";

            var checkboxes = new StackPanel() { Orientation = Orientation.Horizontal };
            var cv = new CheckBox() { IsChecked = ViewModel.Vermieter.Value };
            cv.Click += Checkbox_Click;

            App.Window.RefillCommandContainer(
                new ICommandBarElement[]
                {
                    Filter(),
                    Elements.Filter(ViewModel),
                    AddPerson()
                }); ;
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Vermieter.Value = ((CheckBox)sender).IsChecked ?? false;
        }

        private AppBarButton Filter()
        {
            var Panel = new StackPanel { Orientation = Orientation.Vertical };

            Panel.Children.Add(Elements.CheckBox(ViewModel.Vermieter, "Vermieter"));
            Panel.Children.Add(Elements.CheckBox(ViewModel.Mieter, "Mieter"));
            Panel.Children.Add(Elements.CheckBox(ViewModel.Handwerker, "Handwerker"));

            var Button = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Filter),
                //Label = "Filter",
                Flyout = new Flyout { Content = Panel },
            };
            return Button;
        }

        private AppBarButton AddPerson()
        {
            var Panel = new StackPanel { Orientation = Orientation.Horizontal };

            StackPanel MakePeoplePanel(bool n)
            {
                var buttonpanel = new StackPanel { Orientation = Orientation.Horizontal };
                var symbol = n ? Symbol.Contact : Symbol.People;
                var text = (n ? " Natürliche" : " Juristische") + " Person";
                buttonpanel.Children.Add(new SymbolIcon(Symbol.People));
                buttonpanel.Children.Add(new TextBlock { Text = text });
                return buttonpanel;
            }

            var natButton = new Button { Content = MakePeoplePanel(true) };
            natButton.Click += AddNatuerlichePerson_Click;
            Panel.Children.Add(natButton);
            var jurButton = new Button
            {
                Content = MakePeoplePanel(false),
                Margin = new Thickness(10, 0, 0, 0),
            };
            jurButton.Click += AddJuristischePerson_Click;
            Panel.Children.Add(jurButton);

            var Button = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Kontakt hinzufügen",
                Flyout = new Flyout { Content = Panel },
            };
            return Button;
        }

        private void AddNatuerlichePerson_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NatuerlichePersonDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }

        private void AddJuristischePerson_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(JuristischePersonenDetailPage), null,
                new DrillInNavigationTransitionInfo());
        }
    }
}
