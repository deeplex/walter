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

        public void LoadAdressen(AppViewModel avm)
        {
            Adressen.Value = avm.ctx.Adressen.Select(a => new AdresseViewModel(a, avm)).ToImmutableList();
        }

        public void LoadAnhaenge(IAppImplementation impl, AppViewModel avm)
        {
            Anhaenge.Value = new AnhangListViewModel(impl, avm);
        }

        public SettingsViewModel(IAppImplementation impl, AppViewModel avm)
        {

            try
            {
                LoadAdressen(avm);
                LoadAnhaenge(impl, avm);
            }
            catch (Exception e)
            {
                impl.ShowAlert(e.Message);
            }
        }
    }
}
