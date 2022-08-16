﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed class NotificationService : BindableBase, INotificationService
    {
        public void Navigation<T>(T Element) where T : new()
        {
            App.Window.Navigate(getDetailViewPage(typeof(T)), Element ?? new T());
        }

        private Type getDetailViewPage(Type type)
        {
            return
                type == typeof(Wohnung) ? typeof(WohnungDetailViewPage) :
                type == typeof(NatuerlichePerson) ? typeof(NatuerlichePersonDetailViewPage) :
                type == typeof(JuristischePerson) ? typeof(JuristischePersonDetailViewPage) :
                type == typeof(Vertrag) ? typeof(VertragDetailViewPage) :
                type == typeof(Zaehler) ? typeof(ZaehlerDetailViewPage) :
                type == typeof(Betriebskostenrechnung) ? typeof(BetriebskostenrechnungDetailViewPage) :
                type == typeof(Erhaltungsaufwendung) ? typeof(ErhaltungsaufwendungDetailViewPage) :
                type == typeof(Umlage) ? typeof(UmlageDetailViewPage) :
                null;
        }

        public bool outOfSync { get; set; }
        public void ShowAlert(string text) => ShowAlert(text, text.Length > 20 ? 0 : 3000);
        public void ShowAlert(string text, int ms)
        {
            if (App.Window.Alert == null)
            {
                return;
            }
            App.Window.AlertText.Text = text;
            App.Window.Alert.Show(ms);
        }

        public async Task<bool> Confirmation(string title, string content, string primary, string secondary)
        {
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
        }
        public async Task<bool> Confirmation()
        {
            SetConfirmationDialogText(
                "Sind Sie sicher?",
                "Diese Änderung kann nicht rückgängig gemacht werden.",
                "Ja", "Nein");
            return await ShowConfirmationDialog();
        }

        private async Task<bool> ShowConfirmationDialog()
            => await App.Window.ConfirmationDialog.ShowAsync() == ContentDialogResult.Primary;

        private void SetConfirmationDialogText(string title, string content, string primary, string secondary)
        {
            if (App.Window.ConfirmationDialog is ContentDialog wcd)
            {
                wcd.Title = title;
                wcd.Content = content;
                wcd.PrimaryButtonText = primary;
                wcd.SecondaryButtonText = secondary;
            }
        }
    }
}
