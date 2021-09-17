using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Utils;
using Deeplex.Saverwalter.Print;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.IO;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class VertragCommandBarControl : UserControl
    {
        public VertragCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Vertragdetails"; // TODO Bezeichnung...
        }

        public VertragDetailViewModel ViewModel
        {
            get { return (VertragDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(VertragDetailViewModel),
            typeof(VertragCommandBarControl),
            new PropertyMetadata(null));


        private async void Delete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.SelfDestruct();
            App.Window.AppFrame.GoBack();
        }

        private async void Betriebskostenabrechnung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var Jahr = (int)((Button)sender).CommandParameter;
                await Files.PrintBetriebskostenabrechnung(ViewModel.Entity, Jahr, App.ViewModel, App.Impl);
            }
            catch (Exception ex)
            {
                App.Impl.ShowAlert(ex.Message);
            }
        }
    }
}
