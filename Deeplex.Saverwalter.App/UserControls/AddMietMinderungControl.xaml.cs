using Deeplex.Saverwalter.App.ViewModels.Mieten;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AddMietMinderungControl : UserControl
    {
        public MietMinderungDetailViewModel ViewModel = new MietMinderungDetailViewModel();

        public AddMietMinderungControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(MietMinderungListViewModelProperty, (DepObj, Prop) =>
            {
                ViewModel.VertragId = MietMinderungListViewModel.VertragId;
                ViewModel.Minderung = 0.10;
                ViewModel.Beginn = DateTime.Now.AsUtcKind();
                ViewModel.Ende = DateTime.Now.AsUtcKind();
            });
        }

        public MietMinderungListViewModel MietMinderungListViewModel
        {
            get { return (MietMinderungListViewModel)GetValue(MietMinderungListViewModelProperty); }
            set { SetValue(MietMinderungListViewModelProperty, value); }
        }

        public static readonly DependencyProperty MietMinderungListViewModelProperty
            = DependencyProperty.Register(
            "MietMinderungListViewModel",
            typeof(MietMinderungListViewModel),
            typeof(AddMietMinderungControl),
            new PropertyMetadata(null));

        private void AddMietMinderung_Click(object sender, RoutedEventArgs e)
        {
            App.Walter.MietMinderungen.Add(ViewModel.Entity);
            App.SaveWalter();
            MietMinderungListViewModel.AddToList(ViewModel.Entity);
            var mietminderung = new MietMinderung()
            {
                VertragId = MietMinderungListViewModel.VertragId,
                Minderung = 0.10,
                Beginn = DateTime.Now.AsUtcKind(),
                Ende = DateTime.Now.AsUtcKind(),
            };
            ViewModel = new MietMinderungDetailViewModel(mietminderung);

            if (AddMietMinderungButton.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
    }
}
