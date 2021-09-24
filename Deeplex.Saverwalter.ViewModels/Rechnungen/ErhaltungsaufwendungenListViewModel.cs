using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>> Liste
            = new ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>>();
        public ObservableProperty<bool> disabled = new ObservableProperty<bool>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<ErhaltungsaufwendungenListEntry> AllRelevant { get; set; }

        private ErhaltungsaufwendungenListEntry mSelectedAufwendung;
        public ErhaltungsaufwendungenListEntry SelectedAufwendung
        {
            get => mSelectedAufwendung;
            set
            {
                mSelectedAufwendung = value;
                RaisePropertyChangedAuto();
            }
        }

        public ErhaltungsaufwendungenListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Select(w => new ErhaltungsaufwendungenListEntry(w, avm))
                .ToImmutableList();

            Liste.Value = AllRelevant;
            disabled.Value = false;
        }
    }

    public sealed class ErhaltungsaufwendungenListEntry
    {
        public readonly Erhaltungsaufwendung Entity;
        public string Aussteller => Avm.ctx.FindPerson(Entity.AusstellerId).Bezeichnung;
        public int Id => Entity.ErhaltungsaufwendungId;
        public WohnungListEntry Wohnung;
        public string WohnungString => Wohnung.ToString();
        public string BetragString => Entity.Betrag.ToString() + "€";
        public string Bezeichnung => Entity.Bezeichnung;
        public string DatumString => Entity.Datum.ToString("dd.MM.yyyy");
        private AppViewModel Avm;

        public ErhaltungsaufwendungenListEntry(Erhaltungsaufwendung e, AppViewModel avm)
        {
            Entity = e;
            Avm = avm;
            Wohnung = new WohnungListEntry(Entity.Wohnung, Avm);
        }
    }
}
