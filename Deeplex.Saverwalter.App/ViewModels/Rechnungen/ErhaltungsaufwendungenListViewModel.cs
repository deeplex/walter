using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels.Rechnungen
{
    public sealed class ErhaltungsaufwendungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>> Liste
            = new ObservableProperty<ImmutableList<ErhaltungsaufwendungenListEntry>>();

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

        public ErhaltungsaufwendungenListViewModel()
        {
            AllRelevant = App.Walter.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .ThenInclude(w => w.Adresse)
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
        public WohnungListEntry Wohnung;
        public string WohnungString => Wohnung.ToString();
        public string BetragString => Entity.Betrag.ToString() + "€";
        public string Bezeichnung => Entity.Bezeichnung;
        public string DatumString => Entity.Datum.ToString("dd.MM.yyyy");

        public ErhaltungsaufwendungenListEntry(Erhaltungsaufwendung e)
        {
            Entity = e;
            Wohnung = new WohnungListEntry(Entity.Wohnung);
        }
    }
}
