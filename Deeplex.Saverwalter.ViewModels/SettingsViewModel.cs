using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen =
            new ObservableProperty<ImmutableList<AdresseViewModel>>();
        public ObservableProperty<AnhangListViewModel> Anhaenge =
            new ObservableProperty<AnhangListViewModel>();

        public void LoadAdressen(IAppImplementation impl)
        {
            Adressen.Value = impl.ctx.Adressen.Select(a => new AdresseViewModel(a, impl)).ToImmutableList();
        }

        public void LoadAnhaenge(IAppImplementation impl)
        {
            Anhaenge.Value = new AnhangListViewModel(impl);
        }

        public SettingsViewModel(IAppImplementation impl)
        {

            try
            {
                LoadAdressen(impl);
                LoadAnhaenge(impl);
            }
            catch (Exception e)
            {
                impl.ShowAlert(e.Message);
            }
        }
    }
}
