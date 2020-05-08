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
    public class AnschriftWohnungListXYZ
    {
        public string Key { get; set; }
        public List<WohnungViewModel> Wohnungen { get; set; }
    }

    public class WohnungListViewModel
    {
        public List<WohnungViewModel> Wohnungen { get; }

        public List<AnschriftWohnungListXYZ> AdresseGroup =>
            Wohnungen.GroupBy(w => w.Anschrift).Select(g => new AnschriftWohnungListXYZ { Key = g.Key, Wohnungen = g.ToList() }).ToList();

        public WohnungListViewModel()
        {
            Wohnungen = App.Walter.Wohnungen.Select(w => new WohnungViewModel(w)).ToList();
        }
    }
}
