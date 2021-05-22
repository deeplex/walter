﻿using Deeplex.Utils.ObjectModel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class MainViewModel
    {
        public ObservableProperty<string> Titel = new ObservableProperty<string>();
        public ObservableProperty<AnhangListViewModel> DetailAnhang
            = new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<AnhangListViewModel> ListAnhang
            = new ObservableProperty<AnhangListViewModel>();

        private CommandBar CommandBar { get; set; }
        private InAppNotification SavedIndicator { get; set; }
        private TextBlock SavedIndicatorText { get; set; }
        private ContentDialog ConfirmationDialog { get; set; }
        private SplitView AnhangPane { get; set; }
        private SymbolIcon AnhangSymbol { get; set; }

        public Action<Type, object> Navigate { get; set; }

        public void SetAnhangPane(SplitView arg, SymbolIcon arg2)
        {
            AnhangPane = arg;
            AnhangSymbol = arg2;
        }

        public void OpenAnhang()
        {
            if (!AnhangPane.IsPaneOpen)
            {
                ToggleAnhang();
            }
        }

        public void ToggleAnhang()
        {
            AnhangPane.IsPaneOpen = !AnhangPane.IsPaneOpen;
            AnhangSymbol.Symbol = AnhangPane.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }

        public void SetCommandBar(CommandBar arg)
        {
            CommandBar = arg;
        }

        private void SetConfirmationDialogText(string title, string content, string primary, string secondary)
        {
            ConfirmationDialog.Title = title;
            ConfirmationDialog.Content = content;
            ConfirmationDialog.PrimaryButtonText = primary;
            ConfirmationDialog.SecondaryButtonText = secondary;
        }

        public void SetConfirmationDialog(ContentDialog arg)
        {
            ConfirmationDialog = arg;
        }

        public async Task<bool> Confirmation(string title, string content, string primary, string secondary)
        {
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
        }
        public async Task<bool> Confirmation()
        {
            SetConfirmationDialogText(
                "Bist du sicher?",
                "Diese Änderung kann nicht rückgängig gemacht werden.",
                "Ja.", "Nein.");
            return await ShowConfirmationDialog();
        }

        private async Task<bool> ShowConfirmationDialog()
            => await ConfirmationDialog.ShowAsync() == ContentDialogResult.Primary;

        public void SetSavedIndicator(InAppNotification arg, TextBlock arg2)
        {
            SavedIndicator = arg;
            SavedIndicatorText = arg2;
        }
        public void ShowAlert(string text, int ms = 500)
        {
            if (SavedIndicator == null)
            {
                return;
            }
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
