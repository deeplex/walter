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

        public KontaktListControl()
        {
            InitializeComponent();
            ViewModel = new KontaktListViewModel(App.WalterService, App.NotificationService);
        }

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
            ViewModel.Add.Execute(ViewModel.Selected.Entity);
        }

        private void DataGrid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ViewModel.Selected = (e.OriginalSource as FrameworkElement).DataContext as KontaktListViewModelEntry;
        }

        private async void RemovePerson_Click(object sender, RoutedEventArgs e)
        {
            // TODO No Person is removed here?

            if (await App.NotificationService.Confirmation())
            {
                var guid = ((KontaktListViewModelEntry)((Button)sender).DataContext).Entity.PersonId;

                ViewModel.List.Value = ViewModel.List.Value
                    .Where(k => guid != k.Entity.PersonId).ToImmutableList();
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
            ViewModel.List.Value = (sender as DataGrid).Sort(e.Column, ViewModel.List.Value);
        }
    }
}
