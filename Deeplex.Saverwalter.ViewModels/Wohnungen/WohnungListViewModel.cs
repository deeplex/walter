using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<WohnungListEntry>> Liste = new ObservableProperty<ImmutableList<WohnungListEntry>>();
        public ObservableProperty<WohnungListEntry> SelectedWohnung
            = new ObservableProperty<WohnungListEntry>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<WohnungListEntry> AllRelevant { get; }

        public WohnungListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.Wohnungen
                .Include(w => w.Adresse)
                .Select(w => new WohnungListEntry(w, avm))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }

    public sealed class WohnungListEntry
    {
        public override string ToString() => Anschrift + ", " + Bezeichnung;

        public int Id { get; }
        public Wohnung Entity { get; }
        public Adresse Adresse { get; }
        public string Bezeichnung { get; }
        public string Anschrift { get; }
        public string Besitzer { get; }

        public WohnungListEntry(Wohnung w, AppViewModel avm)
        {
            Id = w.WohnungId;
            Entity = w;
            Adresse = w.Adresse;
            Bezeichnung = w.Bezeichnung;
            Anschrift = AdresseViewModel.Anschrift(w);
            Besitzer = avm.ctx.FindPerson(w.BesitzerId).Bezeichnung;
        }
    }
}
