using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen =
            new ObservableProperty<ImmutableList<AdresseViewModel>>();

        public void LoadAdressen()
        {
            Adressen.Value = App.Walter.Adressen.Select(a => new AdresseViewModel(a)).ToImmutableList();
        }

        public SettingsViewModel()
        {
            try
            {
                LoadAdressen();
            }
            catch
            {
                App.ViewModel.ShowAlert("Keine Datenbank geladen.", 5000);
            }
        }
    }
}
