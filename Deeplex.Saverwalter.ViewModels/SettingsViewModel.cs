using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen =
            new ObservableProperty<ImmutableList<AdresseViewModel>>();

        public void LoadAdressen(IAppImplementation impl)
        {
            Adressen.Value = impl.ctx.Adressen.Select(a => new AdresseViewModel(a, impl)).ToImmutableList();
        }



        public SettingsViewModel(IAppImplementation impl)
        {

            try
            {
                LoadAdressen(impl);
            }
            catch
            {
                impl.ShowAlert("Keine Datenbank geladen.");
            }
        }
    }
}
