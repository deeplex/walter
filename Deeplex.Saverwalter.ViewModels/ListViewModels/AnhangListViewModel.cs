using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    // TODO i18n out of viewmodels...
    public sealed class AnhangListViewModel : ListViewModel<AnhangListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Anhänge";

        public ObservableProperty<ImmutableList<AnhangListViewModelEntry>> Liste = new();

        public IFileService FileService;

        public RelayCommand AddAnhang;

        public void SetList(IAnhang a)
        {
            Entity = a;

            Liste.Value = a.Anhaenge
                .ToList()
                .Select(e => new AnhangListViewModelEntry(e, this))
                .ToImmutableList();
        }

        public IAnhang Entity { get; private set; }

        public AnhangListViewModel(IFileService fs, INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            FileService = fs;
            AddAnhang = new RelayCommand(f => SaveAnhang(f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang(List<Anhang> newFiles = null)
        {
            if (newFiles == null)
            {
                newFiles = (await FileService.pickFiles()).Select(f => Files.SaveAnhang(f, FileService.databaseRoot)).ToList();
            }
            Entity.Anhaenge.AddRange(newFiles);
            WalterDbService.ctx.Anhaenge.AddRange(newFiles);
            WalterDbService.SaveWalter();
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListViewModelEntry(f, self)));
        }

        protected override void updateList()
        {
            throw new System.NotImplementedException();
        }

        public override void SetList()
        {
            throw new System.NotImplementedException();
        }
    }
}
