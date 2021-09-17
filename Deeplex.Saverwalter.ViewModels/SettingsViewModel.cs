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


        public RelayCommand LoadAdressen;
        public RelayCommand LoadAnhaenge;

        public IAppImplementation Impl;
        public AppViewModel Avm;

        public SettingsViewModel(IAppImplementation impl, AppViewModel avm)
        {
            try
            {
                Impl = impl;
                Avm = avm;
                rootPath.Value = avm.root;

                LoadAnhaenge = new RelayCommand(_ =>
                {
                    Anhaenge.Value = new AnhangListViewModel(Impl, Avm);
                }, _ => true);
                LoadAdressen = new RelayCommand(_ =>
                {
                    Adressen.Value = Avm.ctx.Adressen.Select(a => new AdresseViewModel(a, Avm)).ToImmutableList();
                }, _ => true);
            }
            catch (Exception e)
            {
                impl.ShowAlert(e.Message);
            }
        }
    }
}
