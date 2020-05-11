using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public ObservableProperty<List<KontaktListViewModel>> Kontakte { get; }
            = new ObservableProperty<List<KontaktListViewModel>>();

        public ObservableProperty<KontaktListViewModel> SelectedKontakt { get; }
            = new ObservableProperty<KontaktListViewModel>();

        public ObservableProperty<bool> IsLoading { get; }
            = new ObservableProperty<bool>();

        public MainViewModel()
        {
            Kontakte.Value = App.Walter.Kontakte
                .Include(k => k.Adresse)
                .Select(k => new KontaktListViewModel(k))
                .ToList();
        }
    }
}
