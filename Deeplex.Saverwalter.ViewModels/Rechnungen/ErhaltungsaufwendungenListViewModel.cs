using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenListViewModelEntry>> Liste = new();

        public ObservableProperty<string> Filter { get; set; } = new();
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
                .Include(e => e.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Anhaenge)
                .Include(e => e.Wohnung).ThenInclude(w => w.Adresse).ThenInclude(w => w.Anhaenge)
                .Select(w => new ErhaltungsaufwendungenListViewModelEntry(w, db))
                .ToImmutableList();

            Liste.Value = AllRelevant;
        }
    }
}
