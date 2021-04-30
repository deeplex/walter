using Deeplex.Utils.ObjectModel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MainViewModel
    {
        public ObservableProperty<string> Titel = new ObservableProperty<string>();
        public ObservableProperty<AnhangViewModel> Explorer = new ObservableProperty<AnhangViewModel>();

        private CommandBar CommandBar { get; set; }
        private InAppNotification SavedIndicator { get; set; }
        private TextBlock SavedIndicatorText { get; set; }

        public Action<Type, object> Navigate { get; set; }

        public void SetCommandBar(CommandBar arg)
        {
            CommandBar = arg;
        }

        public void SetSavedIndicator(InAppNotification arg, TextBlock arg2)
        {
            SavedIndicator = arg;
            SavedIndicatorText = arg2;
        }
        public void ShowAlert(string text, int ms = 500)
        {
            SavedIndicatorText.Text = text;
            SavedIndicator.Show(ms);
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
