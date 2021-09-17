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
                var b = new Betriebskostenabrechnung(
                    App.ViewModel.ctx,
                    ViewModel.Versionen.Value.First().Id,
                    Jahr,
                    new DateTime(Jahr, 1, 1),
                    new DateTime(Jahr, 12, 31));

                var AuflistungMieter = string.Join(", ", App.Walter.MieterSet
                    .Where(m => m.VertragId == ViewModel.guid).ToList()
                    .Select(a => App.Walter.FindPerson(a.PersonId).Bezeichnung));

                var picker = Utils.Files.FileSavePicker(Path.GetExtension(".docx"));
                picker.SuggestedFileName = Jahr.ToString() + " - " + ViewModel.Wohnung.ToString() + " - " + AuflistungMieter;
                var file = await picker.PickSaveFileAsync();
                var path = Path.Combine(Path.GetDirectoryName(file.Path), Path.GetFileNameWithoutExtension(file.Path));

                b.SaveAsDocx(file.Path);
                b.SaveBetriebskostenabrechnung(path, App.ViewModel);

                App.Impl.ShowAlert("Datei gespeichert unter " + path);
            }
            catch (Exception ex)
            {
                App.Impl.ShowAlert(ex.Message);
            }
        }
    }
}
