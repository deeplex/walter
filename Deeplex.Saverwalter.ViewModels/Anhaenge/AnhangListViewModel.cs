using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    // TODO i18n out of viewmodels...
    public sealed class AnhangListViewModel : BindableBase
    {
        public ObservableProperty<string> Text = new();
        public ObservableProperty<ImmutableList<AnhangListViewModelEntry>> Liste = new();

        public IFileService FileService;
        public IWalterDbService Db;
        public INotificationService NotificationService;

        public RelayCommand AddAnhang;

        private void SetList(IAnhang a)
        {
            var self = this;

            Liste.Value = a.Anhaenge
                .ToList()
                .Select(e => new AnhangListViewModelEntry(e, self))
                .ToImmutableList();
        }

        public IAnhang Entity { get; }

        public AnhangListViewModel(IAnhang a, IFileService fs, INotificationService ns, IWalterDbService db)
        {
            Entity = a;
            Db = db;
            FileService = fs;
            Text.Value = "Anhänge"; // TODO Hard code?
            SetList(a);
            // TODO15
            AddAnhang = new RelayCommand(f => SaveAnhang(f as List<Anhang>), _ => true);
        }

        public async void SaveAnhang(List<Anhang> newFiles = null)
        {
            if (newFiles == null)
            {
                newFiles = (await FileService.pickFiles()).Select(f => Files.SaveAnhang(f, Db.root)).ToList();
            }
            Entity.Anhaenge.AddRange(newFiles);
            Db.ctx.Anhaenge.AddRange(newFiles);
            Db.SaveWalter();
            var self = this;
            newFiles.ForEach(f => Liste.Value = Liste.Value.Add(new AnhangListViewModelEntry(f, self)));
        }
    }
}
