using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen =
            new ObservableProperty<ImmutableList<AdresseViewModel>>();

        public SettingsViewModel()
        {
            Adressen.Value = App.Walter.Adressen.Select(a => new AdresseViewModel(a)).ToImmutableList();
        }
    }
}
