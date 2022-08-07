﻿using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public abstract class ListViewModel<T> : BindableBase, IListViewModel
    {
        public ObservableProperty<ImmutableList<T>> List = new();
        private ImmutableList<T> mAllRelevant { get; set; }
        public ImmutableList<T> AllRelevant {
            get => mAllRelevant;
            protected set
            {
                mAllRelevant = value;
                List.Value = mAllRelevant;
            }
        }
        public T Selected;
        public bool hasSelected => Selected != null;

        protected abstract ImmutableList<T> updateList(string filter);
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
                    List.Value = AllRelevant;
                }
                RaisePropertyChangedAuto();
            }
        }
    }
}
