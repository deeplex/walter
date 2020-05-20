using Deeplex.Saverwalter.App.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class JuristischePersonenListPage : Page
    {
        public JuristischePersonenListViewModel ViewModel = new JuristischePersonenListViewModel();

        public JuristischePersonenListPage()
        {
            InitializeComponent();
        }

        private void EntfernePerson_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var person = (JuristischePersonenPerson)((Button)sender).CommandParameter;
            ViewModel.Personen.Value = ViewModel.Personen.Value.Remove(person);
            App.Walter.JuristischePersonen.Remove(App.Walter.JuristischePersonen.Find(person.Id));
            App.Walter.SaveChanges();
        }
    }
}
