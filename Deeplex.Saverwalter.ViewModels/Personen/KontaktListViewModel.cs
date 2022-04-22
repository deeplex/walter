using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class KontaktListViewModel : BindableBase, IFilterViewModel
    {
        public ObservableProperty<ImmutableList<KontaktListViewModelEntry>> Kontakte = new ObservableProperty<ImmutableList<KontaktListViewModelEntry>>();
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

        public ObservableProperty<string> Filter { get; set; } = new ObservableProperty<string>();
        public ObservableProperty<bool> Vermieter { get; set; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> Mieter { get; set; } = new ObservableProperty<bool>(true);
        public ObservableProperty<bool> Handwerker { get; set; } = new ObservableProperty<bool>(true);
        public ImmutableList<KontaktListViewModelEntry> AllRelevant { get; }

        public KontaktListViewModel(AppViewModel avm)
        {
            AllRelevant = avm.ctx.NatuerlichePersonen
                .Include(k => k.Adresse)
                .Select(k => new KontaktListViewModelEntry(k)).ToImmutableList();

            var jp = avm.ctx.JuristischePersonen;
            foreach (var j in jp)
            {
                AllRelevant = AllRelevant.Add(new KontaktListViewModelEntry(j));
            }

            Kontakte.Value = AllRelevant;
        }
    }
}
