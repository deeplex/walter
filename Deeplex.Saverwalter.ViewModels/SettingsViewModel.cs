using Deeplex.Saverwalter.Services;
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
        public ObservableProperty<ImmutableList<AdresseViewModel>> Adressen = new();
        public ObservableProperty<AnhangListViewModel> Anhaenge = new();
        public ObservableProperty<string> rootPath = new();

        public async Task LoadDatabase()
        {
            rootPath.Value = await Impl.pickFile();
            Db.root = Path.Combine(Path.GetDirectoryName(rootPath.Value), Path.GetFileNameWithoutExtension(rootPath.Value));
            await Db.initializeDatabase(Impl);
        }


        public RelayCommand LoadAdressen;
        public RelayCommand LoadAnhaenge;

        public IAppImplementation Impl;
        public IWalterDbService Db;

        public SettingsViewModel(IAppImplementation impl, IWalterDbService db)
        {
            try
            {
                Impl = impl;
                Db = db;
                rootPath.Value = db.root;

                LoadAnhaenge = new RelayCommand(_ =>
                {
                    Anhaenge.Value = new AnhangListViewModel(Impl, Db);
                }, _ => true);
                LoadAdressen = new RelayCommand(_ =>
                {
                    Adressen.Value = Db.ctx.Adressen.Select(a => new AdresseViewModel(a, Db)).ToImmutableList();
                }, _ => true);
            }
            catch (Exception e)
            {
                impl.ShowAlert(e.Message);
            }
        }
    }
}
