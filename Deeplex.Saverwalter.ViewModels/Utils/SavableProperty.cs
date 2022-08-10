using Deeplex.Utils.ObjectModel;

namespace Deeplex.Saverwalter.ViewModels
{
    public class SavableProperty<T> : ObservableProperty<T>
    {
        DetailViewModel Parent;

        public SavableProperty(DetailViewModel parent, T initial = default) : base(initial)
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
