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
    public sealed class WohnungListViewModel
    {
        public List<WohnungListEntry> Liste = new List<WohnungListEntry>();
        public ObservableProperty<WohnungListEntry> SelectedWohnung
            = new ObservableProperty<WohnungListEntry>();

        public WohnungListViewModel()
        {
            Liste = App.Walter.Wohnungen
                .Select(w => new WohnungListEntry(w))
                .ToList();
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
