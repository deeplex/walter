using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Kontakte = new();
        private KontaktListViewModelEntry mSelectedKontakt;
        public KontaktListViewModelEntry SelectedKontakt
        {
            get => mSelectedKontakt;
            set
            {
                mSelectedKontakt = value;
                RaisePropertyChangedAuto();
                RaisePropertyChanged(nameof(hasSelectedKontakt));
            }
        }
        public bool hasSelectedKontakt => SelectedKontakt != null;

        public ObservableProperty<string> Filter { get; set; } = new();
        public ObservableProperty<bool> Vermieter { get; set; } = new (true);
        public ObservableProperty<bool> Mieter { get; set; } = new (true);
        public ObservableProperty<bool> Handwerker { get; set; } = new (true);
        public ImmutableList<KontaktListViewModelEntry> AllRelevant { get; }

        public KontaktListViewModel(IWalterDbService db)
        {
            AllRelevant = db.ctx.NatuerlichePersonen
                .Include(k => k.Anhaenge)
                .Include(k => k.Adresse).ThenInclude(a => a.Anhaenge)
                .Select(k => new KontaktListViewModelEntry(k)).ToImmutableList();

            var jp = db.ctx.JuristischePersonen
                .Include(j => j.Anhaenge)
                .Include(j => j.Adresse).ThenInclude(a => a.Anhaenge);
            foreach (var j in jp)
            {
                AllRelevant = AllRelevant.Add(new KontaktListViewModelEntry(j));
            }

            Kontakte.Value = AllRelevant;
        }
    }
}
