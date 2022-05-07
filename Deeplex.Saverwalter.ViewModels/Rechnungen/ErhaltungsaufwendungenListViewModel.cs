using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenListViewModelEntry>> Liste
            = new ObservableProperty<ImmutableList<ErhaltungsaufwendungenListViewModelEntry>>();

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ImmutableList<ErhaltungsaufwendungenListViewModelEntry> AllRelevant { get; set; }

        private ErhaltungsaufwendungenListViewModelEntry mSelectedAufwendung;
        public ErhaltungsaufwendungenListViewModelEntry SelectedAufwendung
        {
            get => mSelectedAufwendung;
            set
            {
                mSelectedAufwendung = value;
                RaisePropertyChangedAuto();
            }
        }

        public ErhaltungsaufwendungenListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .ThenInclude(w => w.Adresse)
                .Select(w => new ErhaltungsaufwendungenListViewModelEntry(w, db))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }
}
