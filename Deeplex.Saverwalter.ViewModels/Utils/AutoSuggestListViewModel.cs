using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class AutoSuggestListViewModel
    {
        ImmutableList<AutoSuggestListViewModelEntry> AllEntries { get; set; }
        public ObservableProperty<ImmutableList<AutoSuggestListViewModelEntry>> Entries { get; } = new();
        public void update(string filter)
        {

            if (AllEntries != null)
            {
                if (filter.Trim() == string.Empty)
                {
                    Entries.Value = AllEntries;
                }
                Entries.Value = AllEntries
                    .Where(w => w.ToString().ToLower().Contains(filter.ToLower()))
                    .ToImmutableList();
            }
        }

        public AutoSuggestListViewModel(IWalterDbService db)
        {
            var ctx = db.ctx;
            AllEntries = ctx.Wohnungen
                    .Include(w => w.Adresse)
                    .Select(w => new AutoSuggestListViewModelEntry(w))
                .ToList()
                .Concat(ctx.NatuerlichePersonen
                    .Include(w => w.JuristischePersonen)
                    .Select(w => new AutoSuggestListViewModelEntry(w)))
                .ToList()
                .Concat(ctx.JuristischePersonen
                    .Include(w => w.JuristischePersonen)
                    .Include(w => w.NatuerlicheMitglieder)
                    .Include(w => w.JuristischeMitglieder)
                    .Select(w => new AutoSuggestListViewModelEntry(w)))
                .ToList()
                .Concat(ctx.ZaehlerSet
                    .Select(w => new AutoSuggestListViewModelEntry(w)))
                .ToList()
                .Where(w => w.Bezeichnung != null)
                .ToImmutableList();
            Entries.Value = AllEntries;
        }
    }
}
