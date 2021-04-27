﻿using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class ZaehlerstandListViewModel : BindableBase
    {
        public ObservableProperty<ImmutableList<ZaehlerstandListEntry>> Liste = new ObservableProperty<ImmutableList<ZaehlerstandListEntry>>();
        public int ZaehlerId;

        public ZaehlerstandListViewModel(Zaehler z)
        {
            ZaehlerId = z.ZaehlerId;
            Liste.Value = z.Staende.Select(s => new ZaehlerstandListEntry(s)).ToImmutableList();
        }

        public void AddToList(Zaehlerstand z)
        {
            Liste.Value = Liste.Value.Add(new ZaehlerstandListEntry(z));
        }
    }

    public class ZaehlerstandListEntry
    {
        private Zaehlerstand Entity;
        public int Id => Entity.ZaehlerstandId;
        public double Stand => Entity.Stand;
        public string StandString => Entity.Stand.ToString();
        public DateTimeOffset Datum => Entity.Datum;
        public string DatumString => Datum.ToString("dd.MM.yyyy");

        public ZaehlerstandListEntry(Zaehlerstand z)
        {
            Entity = z;
        }
    }
}
