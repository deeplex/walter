using Deeplex.Utils.ObjectModel;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MainViewModel
    {
        public AnhangViewModel Explorer { get; set; }
        public ObservableProperty<string> Titel = new ObservableProperty<string>();

        private CommandBar CommandBar { get; set; }

        public void SetCommandBar(CommandBar arg)
        {
            CommandBar = arg;
        }


        public MainViewModel()
        {
            Titel.Value = "Walter";
        }

        public void RefillCommandContainer()
        {
            CommandBar.PrimaryCommands.Clear();
            CommandBar.SecondaryCommands.Clear();
        }

        public void RefillCommandContainer(IList<ICommandBarElement> Primary, IList<ICommandBarElement> Secondary = null)
        {
            RefillCommandContainer();
            foreach (var p in Primary)
            {
                CommandBar.PrimaryCommands.Add(p);
            }

            if (Secondary == null) return;

            foreach (var s in Secondary)
            {
                CommandBar.SecondaryCommands.Add(s);
            }
        }
    }
}
