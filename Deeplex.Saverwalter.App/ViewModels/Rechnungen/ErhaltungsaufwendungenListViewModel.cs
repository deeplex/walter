using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels.Rechnungen
{
    public sealed class ErhaltungsaufwendungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>> Liste
            = new ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<ErhaltungsaufwendungenListEntry> AllRelevant { get; set; }

        private BetriebskostenRechnungenListEntry mSelectedAufwendung;
        public BetriebskostenRechnungenListEntry SelectedAufwendung
        {
            get => mSelectedAufwendung;
            set
            {
                mSelectedAufwendung = value;
                RaisePropertyChangedAuto();
            }
        }

        public ErhaltungsaufwendungenListViewModel()
        {
            AllRelevant = App.Walter.Erhaltungsaufwendungen
                .Select(w => new ErhaltungsaufwendungenListEntry(w))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }

    public sealed class ErhaltungsaufwendungenListEntry
    {
        public readonly Erhaltungsaufwendung Entity;
        public string Aussteller => App.Walter.FindPerson(Entity.AusstellerId).Bezeichnung;
        public int Id => Entity.ErhaltungsaufwendungId;
        public string Bezeichnung => Entity.Bezeichnung;
        public string DatumString => Entity.Datum.ToString("dd.mm.yyyy");

        public ErhaltungsaufwendungenListEntry(Erhaltungsaufwendung e)
        {
            Entity = e;
        }
    }
}
