using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class WohnungListViewModel
    {
        public ImmutableDictionary<string, ImmutableList<WohnungListWohnung>> AdresseGroup;

        public WohnungListViewModel()
        {
            AdresseGroup = App.Walter.Wohnungen
                .Include(w => w.Adresse)
                .ToList()
                .Select(w => new WohnungListWohnung(w))
                .GroupBy(w => w.Anschrift)
                .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());
        }
    }

    public class WohnungListWohnung
    {
        public int Id { get; }
        public string Bezeichnung { get; }
        public string Anschrift { get; }

        public WohnungListWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung = w.Bezeichnung;
            Anschrift = AdresseViewModel.Anschrift(w);
        }
    }
}
