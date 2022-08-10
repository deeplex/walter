using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class ListViewModel<IListViewModelEntry> : BindableBase, IListViewModel
    {
        public IWalterDbService WalterDbService { get; protected set; }
        public INotificationService NotificationService { get; protected set; }

        public ObservableProperty<ImmutableList<IListViewModelEntry>> List { get; set; } = new();
        protected IEnumerable<IListViewModelEntry> mAllRelevant { get; set; }
        public IEnumerable<IListViewModelEntry> AllRelevant
        {
            get => mAllRelevant;
            set
            {
                mAllRelevant = value;
                List.Value = mAllRelevant.ToImmutableList();
            }
        }

        public IListViewModelEntry Selected;
        public bool hasSelected => Selected != null;

        public abstract void SetList();

        protected abstract void updateList();
        protected bool applyFilter(params string[] strings)
            => Filter.Split(' ').All(split => strings.Any(str => str.ToLower().Contains(split.ToLower())));

        public RelayCommand Navigate { get; protected set; }

        private string mFilter = "";
        public string Filter
        {
            get => mFilter;
            set
            {
                mFilter = value;

                if (value != "")
                {
                    updateList();
                }
                else
                {
                    List.Value = AllRelevant.ToImmutableList();
                }
                RaisePropertyChangedAuto();
            }
        }
    }

    public interface IListViewModel
    {
        string ToString();
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
        void SetList();
        RelayCommand Navigate { get; }
        string Filter { get; set; }
    }
}
