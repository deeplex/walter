using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class AddMieteControl : UserControl
    {
        public MietenDetailViewModel ViewModel = new MietenDetailViewModel();

        public AddMieteControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(MietenListViewModelProperty, (DepObj, Prop) =>
            {
                ViewModel.VertragId = MietenListViewModel.VertragId;
                ViewModel.Betrag = 0;
                ViewModel.BetreffenderMonat = DateTime.Now.AsUtcKind();
                ViewModel.Zahlungsdatum = DateTime.Now.AsUtcKind();
            });
        }

        public MietenListViewModel MietenListViewModel
        {
            get { return (MietenListViewModel)GetValue(MietenListViewModelProperty); }
            set { SetValue(MietenListViewModelProperty, value); }
        }

        public static readonly DependencyProperty MietenListViewModelProperty
            = DependencyProperty.Register(
            "MietenListViewModel",
            typeof(MietenListViewModel),
            typeof(AddMieteControl),
            new PropertyMetadata(null));

        private void AddMiete_Click(object sender, RoutedEventArgs e)
        {
            App.Walter.Mieten.Add(ViewModel.Entity);
            App.SaveWalter();
            MietenListViewModel.AddToList(ViewModel.Entity);
            var miete = new Miete()
            {
                VertragId = MietenListViewModel.VertragId,
                Betrag = 0,
                BetreffenderMonat = DateTime.Now.AsUtcKind(),
                Zahlungsdatum = DateTime.Now.AsUtcKind(),
            };
            ViewModel = new MietenDetailViewModel(miete);

            if (AddMieteButton.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
    }
}
