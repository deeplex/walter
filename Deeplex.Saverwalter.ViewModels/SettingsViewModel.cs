using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class SettingsViewModel
    {
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen =
            new ObservableProperty<ImmutableList<AdresseViewModel>>();
        public ObservableProperty<AnhangListViewModel> Anhaenge =
            new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<string> rootPath
            = new ObservableProperty<string>();

        public async Task LoadDatabase()
        {
            rootPath.Value = await Impl.pickFile();
            Avm.root = Path.Combine(Path.GetDirectoryName(rootPath.Value), Path.GetFileNameWithoutExtension(rootPath.Value));
            await Avm.initializeDatabase(Impl);
        }


        public void LoadAdressen()
        {
            Adressen.Value = Avm.ctx.Adressen.Select(a => new AdresseViewModel(a, Avm)).ToImmutableList();
        }

        public void LoadAnhaenge()
        {
            Anhaenge.Value = new AnhangListViewModel(Impl, Avm);
        }

        public IAppImplementation Impl;
        public AppViewModel Avm;

        public SettingsViewModel(IAppImplementation impl, AppViewModel avm)
        {
            try
            {
                Impl = impl;
                Avm = avm;
                rootPath.Value = avm.root;
                LoadAdressen();
                LoadAnhaenge();
            }
            catch (Exception e)
            {
                impl.ShowAlert(e.Message);
            }
        }
    }
}
