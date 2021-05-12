﻿using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class KontaktListPage : Page
    {
        public KontaktListViewModel ViewModel = new KontaktListViewModel();

        public KontaktListPage()
        {
            InitializeComponent();
            App.ViewModel.Titel.Value = "Kontakte";

            var checkboxes = new StackPanel() { Orientation = Orientation.Horizontal };
            var cv = new CheckBox() { IsChecked = ViewModel.Vermieter.Value };
            cv.Click += Checkbox_Click;

            App.ViewModel.RefillCommandContainer(
                new ICommandBarElement[]
                {
                    Elements.CheckBox(ViewModel.Vermieter, "Vermieter"),
                    Elements.CheckBox(ViewModel.Mieter, "Mieter"),
                    Elements.CheckBox(ViewModel.Handwerker, "Handwerker"),
                    Elements.Filter(ViewModel),
                    AddPerson()
                });
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Vermieter.Value = ((CheckBox)sender).IsChecked ?? false;
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
