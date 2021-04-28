using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class WohnungListViewModel : IFilterViewModel
    {
        public ObservableProperty<ImmutableList<WohnungListEntry>> Liste = new ObservableProperty<ImmutableList<WohnungListEntry>>();
        public ObservableProperty<WohnungListEntry> SelectedWohnung
            = new ObservableProperty<WohnungListEntry>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<WohnungListEntry> AllRelevant { get; }

        public WohnungListViewModel()
        {
            AllRelevant = App.Walter.Wohnungen
                .Select(w => new WohnungListEntry(w))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }

    public sealed class WohnungListEntry
    {
        public int Id { get; }
        public Wohnung Entity { get; }
        public string Bezeichnung { get; }
        public string Anschrift { get; }

        public WohnungListEntry(Wohnung w)
        {
            Id = w.WohnungId;
            Entity = w;
            Bezeichnung = w.Bezeichnung;
            Anschrift = AdresseViewModel.Anschrift(w);
        }
    }
}
