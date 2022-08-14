﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class BetriebskostenrechnungListViewModel : ListViewModel<BetriebskostenrechnungListViewModelEntry>, IListViewModel
    {
        public override string ToString() => "Betriebskostenrechnungen";

        protected override void updateList()
        {
            List.Value = AllRelevant.Where(v => applyFilter(v.Typ.ToDescriptionString(), v.ToString(), v.BetreffendesJahr.ToString())).ToImmutableList();
        }

        public BetriebskostenrechnungListViewModel(IWalterDbService db, INotificationService ns)
        {
            WalterDbService = db;
            NotificationService = ns;
            Navigate = new RelayCommand(el => ns.Navigation((Betriebskostenrechnung)el), _ => true);
        }

        public override void SetList()
        {
            AllRelevant = transform(include(WalterDbService));
            updateList();
        }

        public void SetList(Vertrag v)
        {
            if (v == null) return;

            AllRelevant = transform(include(WalterDbService)
                    .Where(b => b.Umlage.Wohnungen.Exists(w => v.Wohnung.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Wohnung w)
        {
            if (w == null) return;

            AllRelevant = transform(include(WalterDbService)
                    .Where(b => b.Umlage.Wohnungen.Exists(i => i.WohnungId == w.WohnungId))
                    .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        public void SetList(Umlage u)
        {
            if (u == null) return;

            AllRelevant = transform(include(WalterDbService)
                .Where(e => u.Betriebskostenrechnungen.Exists(i => i.BetriebskostenrechnungId == e.BetriebskostenrechnungId))
                .ToList());
            List.Value = AllRelevant.ToImmutableList();
        }

        private List<Betriebskostenrechnung> include(IWalterDbService db)
        {
            return db.ctx.Betriebskostenrechnungen
                .Include(b => b.Anhaenge)
                .Include(b => b.Umlage).ThenInclude(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Anhaenge)
                .Include(b => b.Umlage).ThenInclude(g => g.Wohnungen).ThenInclude(w => w.Adresse).ThenInclude(a => a.Wohnungen).ThenInclude(w => w.Anhaenge)
                .Include(b => b.Umlage.Zaehler).ThenInclude(b => b.Anhaenge)
                .Include(b => b.Umlage.Wohnungen).ThenInclude(g => g.Adresse).ThenInclude(a => a.Anhaenge)
                .ToList();
        }
        private ImmutableList<BetriebskostenrechnungListViewModelEntry> transform(List<Betriebskostenrechnung> list)
        {
            return list
                .Select(w => new BetriebskostenrechnungListViewModelEntry(w))
                .ToImmutableList();
        }
    }
}
