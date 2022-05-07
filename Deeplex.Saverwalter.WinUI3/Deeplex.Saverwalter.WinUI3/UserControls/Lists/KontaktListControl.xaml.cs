using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using static Deeplex.Saverwalter.WinUI3.Utils.Elements;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class KontaktListControl : UserControl
    {
        public KontaktListViewModel ViewModel { get; set; }

        private void UpdateFilter()
        {
            ViewModel.Kontakte.Value = ViewModel.AllRelevant;

            if (Handwerker == false || Vermieter == false || Mieter == false)
            {
                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value.Where(v =>
                    v.Entity.isHandwerker && Handwerker ||
                    v.Entity.isVermieter && Vermieter ||
                    v.Entity.isMieter && Mieter ||
                    !v.Entity.isMieter && !v.Entity.isVermieter && !v.Entity.isHandwerker)
                    .ToImmutableList();
            }

            if (Kontakte != null)
            {
                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value.Where(v =>
                    Kontakte.Any(k => k.Entity.PersonId == v.Entity.PersonId))
                    .ToImmutableList();
            }

            if (Filter != "")
            {
                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value.Where(v =>
                    applyFilter(Filter, v.Anschrift, v.Name, v.Vorname, v.Email, v.Telefon))
                    .ToImmutableList();
            }
        }

        public KontaktListControl()
        {
            InitializeComponent();
            ViewModel = new KontaktListViewModel(App.WalterService);
            RegisterPropertyChangedCallback(FilterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(KontakteProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(VermieterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(MieterProperty, (DepObj, Prop) => UpdateFilter());
            RegisterPropertyChangedCallback(HandwerkerProperty, (DepObj, Prop) => UpdateFilter());
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty
            = DependencyProperty.Register(
                  "Filter",
                  typeof(string),
                  typeof(KontaktListControl),
                  new PropertyMetadata(""));

        public bool Vermieter
        {
            get { return (bool)GetValue(VermieterProperty); }
            set { SetValue(VermieterProperty, value); }
        }
        public static readonly DependencyProperty VermieterProperty
            = DependencyProperty.Register(
                  "Vermieter",
                  typeof(bool),
                  typeof(KontaktListControl),
                  new PropertyMetadata(true));

        public bool Mieter
        {
            get { return (bool)GetValue(MieterProperty); }
            set { SetValue(MieterProperty, value); }
        }
        public static readonly DependencyProperty MieterProperty
            = DependencyProperty.Register(
                  "Mieter",
                  typeof(bool),
                  typeof(KontaktListControl),
                  new PropertyMetadata(true));

        public bool Handwerker
        {
            get { return (bool)GetValue(HandwerkerProperty); }
            set { SetValue(HandwerkerProperty, value); }
        }
        public static readonly DependencyProperty HandwerkerProperty
            = DependencyProperty.Register(
                  "Handwerker",
                  typeof(bool),
                  typeof(KontaktListControl),
                  new PropertyMetadata(true));

        public ImmutableList<KontaktListViewModelEntry> Kontakte
        {
            get { return (ImmutableList<KontaktListViewModelEntry>)GetValue(KontakteProperty); }
            set { SetValue(KontakteProperty, value); }
        }

        public static readonly DependencyProperty KontakteProperty
            = DependencyProperty.Register(
                  "KontakteProperty",
                  typeof(ImmutableList<KontaktListViewModelEntry>),
                  typeof(KontaktListControl),
                  new PropertyMetadata(null));

        public bool DeleteBool => VertragGuid != Guid.Empty || JuristischePersonId != 0;

        public Guid VertragGuid
        {
            get { return (Guid)GetValue(VertragGuidProperty); }
            set { SetValue(VertragGuidProperty, value); }
        }

        public static readonly DependencyProperty VertragGuidProperty
            = DependencyProperty.Register(
                  "VertragGuid",
                  typeof(Guid),
                  typeof(KontaktListControl),
                  new PropertyMetadata(Guid.Empty));

        public int JuristischePersonId
        {
            get { return (int)GetValue(JuristischePersonIdProperty); }
            set { SetValue(JuristischePersonIdProperty, value); }
        }

        public static readonly DependencyProperty JuristischePersonIdProperty
            = DependencyProperty.Register(
                  "JuristischePersonId",
                  typeof(int),
                  typeof(KontaktListControl),
                  new PropertyMetadata(0));

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var sk = ViewModel.SelectedKontakt;
            if (sk != null)
            {
                var target =
                    sk.Type == typeof(NatuerlichePerson) ? typeof(NatuerlichePersonDetailViewPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailViewPage) :
                    null;

                App.Window.Navigate(target, App.WalterService.ctx.FindPerson(sk.Entity.PersonId));
            }
        }

        private void DataGrid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ViewModel.SelectedKontakt = (e.OriginalSource as FrameworkElement).DataContext as KontaktListViewModelEntry;
        }

        private async void RemovePerson_Click(object sender, RoutedEventArgs e)
        {
            if (await App.NotificationService.Confirmation())
            {
                var guid = ((KontaktListViewModelEntry)((Button)sender).DataContext).Entity.PersonId;

                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value
                    .Where(k => guid != k.Entity.PersonId).ToImmutableList();

                if (VertragGuid != Guid.Empty)
                {
                    App.WalterService.ctx.MieterSet
                        .Where(m => m.PersonId == guid && m.VertragId == VertragGuid)
                        .ToList().ForEach(m => App.WalterService.ctx.MieterSet.Remove(m));
                }

                if (JuristischePersonId != 0)
                {
                    App.WalterService.ctx.JuristischePersonenMitglieder
                        .Where(m => m.PersonId == guid && m.JuristischePersonId == JuristischePersonId)
                        .ToList().ForEach(m => App.WalterService.ctx.JuristischePersonenMitglieder.Remove(m));
                }

                App.WalterService.SaveWalter();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((KontaktListViewModelEntry)((DataGrid)sender).SelectedItem)?.Entity;
            if (a is NatuerlichePerson n)
            {
                App.Window.ListAnhang.Value = new AnhangListViewModel(n, App.FileService, App.NotificationService, App.WalterService);
            }
            else if (a is JuristischePerson j)
            {
                App.Window.ListAnhang.Value = new AnhangListViewModel(j, App.FileService, App.NotificationService, App.WalterService);
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Kontakte.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Kontakte.Value);
        }
    }
}
