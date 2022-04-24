﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class MietenListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<MietenListViewModelEntry>> Liste = new();
        public Guid VertragId;

        public IWalterDbService Db;
        public IAppImplementation Impl;

        public MietenListViewModel(Guid VertragGuid, IAppImplementation impl, IWalterDbService db)
        {
            VertragId = VertragGuid;
            Db = db;
            Impl = impl;
            var self = this;
            Liste.Value = Db.ctx.Mieten
                .Where(m => m.VertragId == VertragGuid)
                .Select(m => new MietenListViewModelEntry(m, self))
                .ToImmutableList();
        }

        public void AddToList(Miete z)
        {
            Liste.Value = Liste.Value.Add(new MietenListViewModelEntry(z, this));
        }
    }
}
