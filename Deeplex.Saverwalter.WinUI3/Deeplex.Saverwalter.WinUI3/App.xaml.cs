﻿using Deeplex.Saverwalter.WinUI3.Utils;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Deeplex.Saverwalter.WinUI3
{
    sealed partial class App : Application
    {
        public static AppViewModel ViewModel { get; private set; }

        public static SaverwalterContext Walter => ViewModel.ctx;
        public static void SaveWalter() => ViewModel.SaveWalter();

        public App()
        {
            // TODO: remove... This is to check where Walter has to go... Check todo in Model.cs aswell.
            var a = Package.Current.InstalledLocation.Path;
            var b = ApplicationData.Current.LocalFolder.Path;

            InitializeComponent();
        }

        public static async Task CopyDataBase()
        {
            var picker = Files.FilePicker(".db");
            picker.CommitButtonText = "Lade Datenbank";
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db");
                var stream = await file.OpenStreamForReadAsync();
                var bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                var ok = Saverwalter.ViewModels.Utils.Files.MakeSpace(path);
                if (ok)
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var newFile = await folder.CreateFileAsync("walter.db");
                    using (var writer = await newFile.OpenStreamForWriteAsync())
                    {
                        await writer.WriteAsync(bytes, 0, bytes.Length);
                    }

                    if (ViewModel.ctx != null)
                    {
                        ViewModel.ctx.Dispose();
                    }
                }
            }

            LoadDataBase();
        }

        public static async Task InitializeDatabase()
        {
            if (ViewModel.ctx != null) return;
            if (File.Exists(System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db")))
            {
                ViewModel.ctx?.Dispose();
                LoadDataBase();
            }
            else if (await ViewModel.Confirmation(
                "Keine Datenbank gefunden",
                "Datenbank suchen, oder leere Datenbank erstellen?",
                "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank"))
            {
                await CopyDataBase();
            }
            else
            {
                ViewModel.ctx?.Dispose();
                LoadDataBase();
            }
        }

        public static void LoadDataBase()
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
                optionsBuilder.UseSqlite("Data Source=" + System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "walter.db"));
                ViewModel.ctx = new SaverwalterContext(optionsBuilder.Options);
                ViewModel.ctx.Database.Migrate();
            }
            catch
            {
                try
                {
                    ViewModel?.ShowAlert("Konnte Datenbank nicht laden.");
                }
                catch { }
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            ViewModel = new AppViewModel();

            m_window = new MainWindow();
            m_window.Activate();

            ViewModel.updateAutoSuggestEntries();
        }

        private Window m_window;
    }

    public abstract partial class AppImplementation : IAppImplementation
    {
        public SaverwalterContext ctx { get; set; }
        public ObservableProperty<string> Titel { get; set; } = new ObservableProperty<string>();
        protected CommunityToolkit.WinUI.UI.Controls.InAppNotification SavedIndicator { get; set; }
        protected TextBlock SavedIndicatorText { get; set; }
        protected ContentDialog ConfirmationDialog { get; set; }
        protected SplitView AnhangPane { get; set; }
        protected SymbolIcon AnhangSymbol { get; set; }

        public void SaveWalter()
        {
            ctx.SaveChanges();
            ShowAlert("Gespeichert", 1000);
        }

        public async Task<bool> Confirmation()
        {
            SetConfirmationDialogText(
                "Bist du sicher?",
                "Diese Änderung kann nicht rückgängig gemacht werden.",
                "Ja", "Nein");
            return await ShowConfirmationDialog();
        }
        public async Task<bool> Confirmation(string title, string content, string primary, string secondary)
        {
            if (ConfirmationDialog == null)
            {
                throw new Exception("Confirmation dialog is not defined.");
            }
            SetConfirmationDialogText(title, content, primary, secondary);
            return await ShowConfirmationDialog();
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

        private async Task<bool> ShowConfirmationDialog()
            => await ConfirmationDialog.ShowAsync() == ContentDialogResult.Primary;

        private void SetConfirmationDialogText(string title, string content, string primary, string secondary)
        {
            if (ConfirmationDialog != null)
            {
                ConfirmationDialog.Title = title;
                ConfirmationDialog.Content = content;
                ConfirmationDialog.PrimaryButtonText = primary;
                ConfirmationDialog.SecondaryButtonText = secondary;
            }
        }
    }

    public sealed class AppViewModel : AppImplementation
    {
        public void updateAutoSuggestEntries()
        {
            if (ctx != null)
            {
                AllAutoSuggestEntries.Value = ctx.Wohnungen.Include(w => w.Adresse).Select(w => new AutoSuggestEntry(w)).ToList()
                    .Concat(ctx.NatuerlichePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                    .Concat(ctx.JuristischePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                    .Concat(ctx.Vertraege
                        .Include(w => w.Wohnung)
                        .Where(w => w.Ende == null || w.Ende < DateTime.Now)
                        .Select(w => new AutoSuggestEntry(w))).ToList()
                    .Concat(ctx.ZaehlerSet.Select(w => new AutoSuggestEntry(w))).ToList()
                    .Concat(ctx.Betriebskostenrechnungen.Include(w => w.Gruppen)
                        .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestEntry(w))).ToList()
                    .Where(w => w.Bezeichnung != null).ToImmutableList();
                AutoSuggestEntries.Value = AllAutoSuggestEntries.Value;
            }
        }

        public void updateAutoSuggestEntries(string filter)
        {
            AutoSuggestEntries.Value = AllAutoSuggestEntries.Value.Where(w => w.ToString().ToLower().Contains(filter.ToLower())).ToImmutableList();
        }

        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AllAutoSuggestEntries
         = new ObservableProperty<ImmutableList<AutoSuggestEntry>>();
        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries
            = new ObservableProperty<ImmutableList<AutoSuggestEntry>>();

        public AppViewModel()
        {
            Titel.Value = "Walter";
        }

        private CommandBar CommandBar { get; set; }

        public ObservableProperty<AnhangListViewModel> DetailAnhang
            = new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<AnhangListViewModel> ListAnhang
            = new ObservableProperty<AnhangListViewModel>();

        public void clearAnhang()
        {
            updateListAnhang(null);
            updateDetailAnhang(null);
        }
        public void updateListAnhang(AnhangListViewModel list) => updateAnhang(ListAnhang, list);
        public void updateDetailAnhang(AnhangListViewModel detail) => updateAnhang(DetailAnhang, detail);

        private void updateAnhang(ObservableProperty<AnhangListViewModel> op, AnhangListViewModel a)
        {
            op.Value = a;
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

        public void SetAnhangPane(SplitView arg, SymbolIcon arg2)
        {
            AnhangPane = arg;
            AnhangSymbol = arg2;
        }

        public void SetCommandBar(CommandBar arg)
        {
            CommandBar = arg;
        }

        public void SetConfirmationDialog(ContentDialog arg)
        {
            ConfirmationDialog = arg;
        }

        public void SetSavedIndicator(CommunityToolkit.WinUI.UI.Controls.InAppNotification arg, TextBlock arg2)
        {
            SavedIndicator = arg;
            SavedIndicatorText = arg2;
        }

        public Action<Type, object> Navigate { get; set; }
    }
}

