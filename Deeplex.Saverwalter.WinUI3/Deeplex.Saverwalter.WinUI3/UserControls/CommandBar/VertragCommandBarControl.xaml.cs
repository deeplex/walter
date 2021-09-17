using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;


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
            throw new NotImplementedException();
            //try
            //{
            //    var Jahr = (int)((Button)sender).CommandParameter;
            //    var b = new Betriebskostenabrechnung(
            //        App.Walter,
            //        ViewModel.Versionen.Value.First().Id,
            //        Jahr,
            //        new DateTime(Jahr, 1, 1),
            //        new DateTime(Jahr, 12, 31));

            //    var AuflistungMieter = string.Join(", ", App.Walter.MieterSet
            //        .Where(m => m.VertragId == ViewModel.guid).ToList()
            //        .Select(a => App.Walter.FindPerson(a.PersonId).Bezeichnung));

            //    var s = Jahr.ToString() + " - " + ViewModel.Wohnung.ToString() + " - " + AuflistungMieter;
            //    var path = ApplicationData.Current.LocalFolder.Path + @"\" + s;

            //    var worked = b.SaveAsDocx(path + ".docx");
            //    var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";
            //    var anhang = Saverwalter.ViewModels.Utils.Files.ExtractFrom(path + ".docx");

            //    if (anhang != null)
            //    {
            //        App.Walter.VertragAnhaenge.Add(new VertragAnhang()
            //        {
            //            Anhang = anhang,
            //            Target = ViewModel.guid,
            //        });
            //        App.SaveWalter();
            //        App.ViewModel.DetailAnhang.Value.AddAnhangToList(anhang);
            //        App.ViewModel.ShowAlert(text, 5000);
            //    }

            //    // TODO b.SaveAnhaenge(path);

            //    App.ViewModel.ShowAlert(text, 5000);
            //}
            //catch (Exception ex)
            //{
            //    App.ViewModel.ShowAlert(ex.Message, 5000);
            //}
        }
    }
}
