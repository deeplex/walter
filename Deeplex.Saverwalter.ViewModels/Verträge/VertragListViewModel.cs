﻿using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class VertragListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<VertragListViewModelVertrag>> Vertraege = new();
        public ObservableProperty<VertragListViewModelVertrag> SelectedVertrag = new();
        public ObservableProperty<bool> OnlyActive = new();

        public ObservableProperty<string> Filter { get; set; } = new();
        public ImmutableList<VertragListViewModelVertrag> AllRelevant { get; set; }

        public VertragListViewModel(IWalterDbService db)
        {
            AllRelevant = Vertraege.Value = db.ctx.Vertraege
                .Include(v => v.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(v => v.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(v => v.Anhaenge)
                .ToList()
                .GroupBy(v => v.VertragId)
                .Select(v => new VertragListViewModelVertrag(v, db))
                .OrderBy(v => v.Beginn).Reverse()
                .ToImmutableList();
            Vertraege.Value = AllRelevant;
        }
    }
}
