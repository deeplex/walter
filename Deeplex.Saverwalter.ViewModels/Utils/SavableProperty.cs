using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public class SavableProperty<T, U> : ObservableProperty<T>
    {
        IDetailViewModel<U> Parent;

        public SavableProperty(IDetailViewModel<U> parent, T initial = default) : base(initial)
        {
            Parent = parent;
        }

        public new T Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                Parent.checkForChanges();
            }
        }
    }

    public class SavableEntryProperty<T, U> : ObservableProperty<T>
    {
        IListViewModelEntry<U> Parent;

        public SavableEntryProperty(IListViewModelEntry<U> parent, T initial = default) : base(initial)
        {
            Parent = parent;
        }

        public new T Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                Parent.checkForChanges();
            }
        }
    }
}
