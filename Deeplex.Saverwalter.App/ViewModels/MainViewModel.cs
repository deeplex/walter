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
        public ObservableProperty<List<KontaktViewModel>> Kontakte { get; }
            = new ObservableProperty<List<KontaktViewModel>>();

        public ObservableProperty<KontaktViewModel> SelectedKontakt { get; }
            = new ObservableProperty<KontaktViewModel>();

        public ObservableProperty<bool> IsLoading { get; }
            = new ObservableProperty<bool>();

        public MainViewModel()
        {
            Kontakte.Value = App.Walter.Kontakte
                .Include(k => k.Adresse)
                .Select(k => new KontaktViewModel(k))
                .ToList();
        }
    }
}
