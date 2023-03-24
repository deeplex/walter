using System.Reflection;
using System.Windows.Input;

namespace Deeplex.Utils.ObjectModel
{
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Predicate<T> mCanExecute;
        private readonly Func<T, Task> mExecute;
        private Task mTask;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            mExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            mCanExecute = canExecute;
        }

        public async void Execute(object parameter)
        {
            mTask = mExecute((T)parameter);
            RaiseCanExecuteChanged(new EventArgs());
            await mTask;
            mTask = null;
            RaiseCanExecuteChanged(new EventArgs());
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => mTask == null && (parameter is T
                || (!typeof(T).GetTypeInfo()
                        .IsValueType && parameter == null)) // if T isn't a value type, parameter may also equal null
               && (mCanExecute == null || mCanExecute((T)parameter));


        public void RaiseCanExecuteChanged(EventArgs args)
            => CanExecuteChanged?.Invoke(this, args);
    }

    public class AsyncRelayCommand : AsyncRelayCommand<object>
    {
        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        {
        }
    }
}
