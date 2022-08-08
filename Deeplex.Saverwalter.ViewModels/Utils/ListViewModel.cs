using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class ListViewModel<IListViewModelEntry> : BindableBase
    {
        public ObservableProperty<ImmutableList<IListViewModelEntry>> List { get; set; } = new();
        protected IEnumerable<IListViewModelEntry> mAllRelevant { get; set; }
        public IEnumerable<IListViewModelEntry> AllRelevant {
            get => mAllRelevant;
            set
            {
                mAllRelevant = value;
                List.Value = mAllRelevant.ToImmutableList();
            }
        }

        public IListViewModelEntry Selected;
        public bool hasSelected => Selected != null;

        protected abstract ImmutableList<IListViewModelEntry> updateList(string filter);
        protected static bool applyFilter(string filter, params string[] strings) =>
            filter.Split(' ').All(split => strings.Any(str => str.ToLower().Contains(split.ToLower())));

        public RelayCommand Navigate { get; protected set; }

        private string mFilter;
        public string Filter
        {
            get => mFilter;
            set
            {
                mFilter = value;

                if (value != "")
                {
                    List.Value = updateList(value);
                }
                else
                {
                    List.Value = AllRelevant.ToImmutableList();
                }
                RaisePropertyChangedAuto();
            }
        }
    }
}
