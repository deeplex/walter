using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class MainViewModel : BindableBase
    {
        // TODO should get Kontakte Async and stuff. In an getKontakteAsyncFunc? 
        public ObservableProperty<List<KontaktListViewModel>> Kontakte { get; }
            = new ObservableProperty<List<KontaktListViewModel>>();

        public ObservableProperty<KontaktListViewModel> SelectedKontakt { get; }
            = new ObservableProperty<KontaktListViewModel>();

        public ObservableProperty<bool> IsLoading { get; }
            = new ObservableProperty<bool>();

        public MainViewModel()
        {
            Kontakte.Value = App.Walter.Kontakte
                .Select(k => new KontaktListViewModel(k))
                .ToList();
        }
    }
}
