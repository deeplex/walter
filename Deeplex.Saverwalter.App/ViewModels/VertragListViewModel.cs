using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragListViewModel
    {
        public List<VertragViewModel> Vertraege { get; } = new List<VertragViewModel>();

        public VertragListViewModel()
        {
            Vertraege = App.Walter.Vertraege
                .Include(v => v.Wohnung)
                .Include(v => v.Mieter).ThenInclude(m => m.Kontakt)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(g => new VertragViewModel(g))
                .OrderBy(v => v.Beginn.Value)
                .Reverse() // Newest first.
                .ToList();
        }
    }
}
