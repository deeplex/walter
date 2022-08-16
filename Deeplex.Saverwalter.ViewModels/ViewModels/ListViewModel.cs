using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class ListViewModel<T> : BindableBase, IListViewModel
    {
        public IWalterDbService WalterDbService { get; protected set; }
        public INotificationService NotificationService { get; protected set; }

        public ObservableProperty<ImmutableList<T>> List { get; set; } = new();
        protected IEnumerable<T> mAllRelevant { get; set; }
        public IEnumerable<T> AllRelevant
        {
            get => mAllRelevant;
            set
            {
                mAllRelevant = value;
                List.Value = mAllRelevant.ToImmutableList();
            }
        }

        public T Selected { get; set; }
        public bool hasSelected => Selected != null;

        public abstract void SetList();

        protected abstract void updateList();
        protected bool applyFilter(params string[] strings)
            => Filter.Split(' ').All(split => strings.Any(str => str != null && str.ToLower().Contains(split.ToLower())));

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
