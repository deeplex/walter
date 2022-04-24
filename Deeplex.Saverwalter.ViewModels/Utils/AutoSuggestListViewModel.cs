using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
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
            AllEntries = ctx.Wohnungen.Include(w => w.Adresse).Select(w => new AutoSuggestListViewModelEntry(w)).ToList()
                .Concat(ctx.NatuerlichePersonen.Select(w => new AutoSuggestListViewModelEntry(w))).ToList()
                .Concat(ctx.JuristischePersonen.Select(w => new AutoSuggestListViewModelEntry(w))).ToList()
                .Concat(ctx.Vertraege.Include(w => w.Wohnung)
                        .Where(w => w.Ende == null || w.Ende < DateTime.Now)
                        .Select(w => new AutoSuggestListViewModelEntry(w))).ToList()
                .Concat(ctx.ZaehlerSet.Select(w => new AutoSuggestListViewModelEntry(w))).ToList()
                .Concat(ctx.Betriebskostenrechnungen.Include(w => w.Gruppen)
                    .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestListViewModelEntry(w))).ToList()
                    .Where(w => w.Bezeichnung != null).ToImmutableList();
            Entries.Value = AllEntries;
        }
    }
}
