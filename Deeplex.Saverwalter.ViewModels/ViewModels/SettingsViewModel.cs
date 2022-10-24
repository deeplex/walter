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

        public async Task<string> LoadDatabase()
        {
            rootPath.Value = await FileService.pickFile();
            FileService.databaseRoot = Path.Combine(Path.GetDirectoryName(rootPath.Value), Path.GetFileNameWithoutExtension(rootPath.Value));
            // TODO this has to be implemented somehow.
            //await App.initializeDatabase(Impl);
            return FileService.databaseRoot;
        }

        public RelayCommand LoadAdressen;
        public RelayCommand LoadAnhaenge;

        public IFileService FileService;
        public IWalterDbService Db;

        public SettingsViewModel(IFileService fs, INotificationService ns, IWalterDbService db)
        {
            try
            {
                FileService = fs;
                Db = db;
                rootPath.Value = fs.databaseRoot;

                // TODO refactor
                LoadAnhaenge = new RelayCommand(_ =>
                {
                    ns.ShowAlert("Not implemented"); // TODO
                                                     //    Anhaenge.Value = new AnhangListViewModel(fs, ns, Db);
                }, _ => true);
                LoadAdressen = new RelayCommand(_ =>
                {
                    Adressen.Value = Db.ctx.Adressen.Select(a => new AdresseViewModel(a, Db, ns)).ToImmutableList();
                }, _ => true);
            }
            catch (Exception)
            {
                // TODO
                //impl.ShowAlert(e.Message);
            }
        }
    }
}
