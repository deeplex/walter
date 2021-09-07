using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.App.Views;
using Deeplex.Saverwalter.Model;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Deeplex.Saverwalter.App.Utils.Elements;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
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
                    Kontakte.Any(k => k.Guid == v.Guid))
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
            ViewModel = new KontaktListViewModel(App.ViewModel);
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

        public ImmutableList<KontaktListEntry> Kontakte
        {
            get { return (ImmutableList<KontaktListEntry>)GetValue(KontakteProperty); }
            set { SetValue(KontakteProperty, value); }
        }

        public static readonly DependencyProperty KontakteProperty
            = DependencyProperty.Register(
                  "KontakteProperty",
                  typeof(ImmutableList<KontaktListEntry>),
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
                    sk.Type == typeof(NatuerlichePerson) ? typeof(NatuerlichePersonDetailPage) :
                    sk.Type == typeof(JuristischePerson) ? typeof(JuristischePersonenDetailPage) :
                    null;

                App.ViewModel.Navigate(target, App.Walter.FindPerson(sk.Guid));
            }
        }

        private void DataGrid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ViewModel.SelectedKontakt = (e.OriginalSource as FrameworkElement).DataContext as KontaktListEntry;
        }

        private async void RemovePerson_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ViewModel.Confirmation())
            {
                var guid = ((KontaktListEntry)((Button)sender).DataContext).Guid;

                ViewModel.Kontakte.Value = ViewModel.Kontakte.Value
                    .Where(k => guid != k.Guid).ToImmutableList();

                if (VertragGuid != Guid.Empty)
                {
                    App.Walter.MieterSet
                        .Where(m => m.PersonId == guid && m.VertragId == VertragGuid)
                        .ToList().ForEach(m => App.Walter.MieterSet.Remove(m));
                }

                if (JuristischePersonId != 0)
                {
                    App.Walter.JuristischePersonenMitglieder
                        .Where(m => m.PersonId == guid && m.JuristischePersonId == JuristischePersonId)
                        .ToList().ForEach(m => App.Walter.JuristischePersonenMitglieder.Remove(m));
                }

                App.SaveWalter();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = ((KontaktListEntry)((DataGrid)sender).SelectedItem)?.Entity;
            if (a is NatuerlichePerson n)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(n, App.ViewModel));
            }
            else if (a is JuristischePerson j)
            {
                App.ViewModel.updateListAnhang(new AnhangListViewModel(j, App.ViewModel));
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            ViewModel.Kontakte.Value = (sender as DataGrid).Sort(e.Column, ViewModel.Kontakte.Value);
        }
    }
}
