using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktListViewModel
    {
        public List<KontaktViewModel> Kontakte { get; } = new List<KontaktViewModel>();

        public KontaktListViewModel(SaverwalterContext Walter)
        {
            Kontakte = Walter.Kontakte.Include(k => k.Adresse).Select(k => new KontaktViewModel(k)).ToList();
        }
    }
}
