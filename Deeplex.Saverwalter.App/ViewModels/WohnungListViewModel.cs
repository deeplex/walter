using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deeplex.Saverwalter.App.ViewModels;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungListViewModel
    {
        public int Id { get; }
        public ObservableProperty<string> Bezeichnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift { get; } = new ObservableProperty<string>();

        public WohnungListViewModel(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung.Value = w.Bezeichnung;
            Anschrift.Value = Utils.Anschrift(w);
        }
    }
}
