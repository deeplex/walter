using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ContextMenuControl : UserControl
    {
        public ContextMenuControl()
        {
            InitializeComponent();
        }

        public object Item
        {
            get { return (object)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty
            = DependencyProperty.Register(
            "ItemProperty",
            typeof(object),
            typeof(ContextMenuControl),
            new PropertyMetadata(null));

        public bool Anhang
        {
            get { return (bool)GetValue(AnhangProperty); }
            set { SetValue(AnhangProperty, value); }
        }

        public static readonly DependencyProperty AnhangProperty
            = DependencyProperty.Register(
            "Anhang",
            typeof(bool),
            typeof(ContextMenuControl),
            new PropertyMetadata(false));

        public bool Navigation
        {
            get { return (bool)GetValue(NavigationProperty); }
            set { SetValue(NavigationProperty, value); }
        }

        public static readonly DependencyProperty NavigationProperty
            = DependencyProperty.Register(
            "Navigation",
            typeof(bool),
            typeof(ContextMenuControl),
            new PropertyMetadata(false));

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            if (Item is WohnungListViewModelEntry w)
            {
                App.Window.Navigate(typeof(WohnungDetailPage), w.Entity);
            }
            else if (Item is KontaktListViewModelEntry k)
            {
                var a = App.Walter.NatuerlichePersonen.SingleOrDefault(p => p.PersonId == k.Entity.PersonId);
                if (a != null)
                {
                    App.Window.Navigate(typeof(NatuerlichePersonDetailPage), a);
                }
                else
                {
                    var b = App.Walter.JuristischePersonen.SingleOrDefault(p => p.PersonId == k.Entity.PersonId);
                    if (b != null)
                    {
                        App.Window.Navigate(typeof(JuristischePersonenDetailPage), b);
                    }
                }
            }
            else if (Item is ZaehlerListViewModelEntry z)
            {
                App.Window.Navigate(typeof(ZaehlerDetailPage), z.Entity);
            }
        }

        private void Anhaenge_Click(object sender, RoutedEventArgs e)
        {
            if (Item is WohnungListViewModelEntry w)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(w.Entity, App.Impl, App.ViewModel));
            }
            else if (Item is KontaktListViewModelEntry k)
            {
                var a = App.Walter.NatuerlichePersonen.SingleOrDefault(p => p.PersonId == k.Entity.PersonId);
                if (a != null)
                {
                    App.ViewModel.updateListAnhang(new AnhangListViewModel(a, App.Impl, App.ViewModel));
                }
                else
                {
                    var b = App.Walter.JuristischePersonen.SingleOrDefault(p => p.PersonId == k.Entity.PersonId);
                    if (b != null)
                    {
                        App.ViewModel.updateListAnhang(new AnhangListViewModel(b, App.Impl, App.ViewModel));
                    }
                }
            }
            else if (Item is ZaehlerstandListViewModelEntry z)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(z.Entity, App.Impl, App.ViewModel));
            }
        }
    }
}
