using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class WohnungDetailCommandBarControl : UserControl
    {
        public WohnungDetailCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Wohnung"; // TODO Bezeichnung
        }

        public WohnungDetailViewModel ViewModel
        {
            get { return (WohnungDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(WohnungDetailViewModel),
            typeof(WohnungDetailCommandBarControl),
            new PropertyMetadata(null));

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.selfDestruct();
            App.Window.AppFrame.GoBack();
        }

        private void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //var Jahr = (int)((Button)sender).CommandParameter;
            //var l = new ErhaltungsaufwendungWohnung(App.Walter, ViewModel.Id, Jahr);

            //var s = Jahr.ToString() + " - " + ViewModel.Anschrift;
            //var path = ApplicationData.Current.TemporaryFolder.Path + @"\" + s;

            //var worked = l.SaveAsDocx(path + ".docx");
            //var text = worked ? "Datei gespeichert als: " + s : "Datei konnte nicht gespeichert werden.";

            //var anhang = Saverwalter.ViewModels.Utils.Files.ExtractFrom(path + ".docx");

            //if (anhang != null)
            //{
            //    App.Walter.WohnungAnhaenge.Add(new WohnungAnhang()
            //    {
            //        Anhang = anhang,
            //        Target = ViewModel.Entity,
            //    });
            //    App.SaveWalter();
            //    App.ViewModel.DetailAnhang.Value.AddAnhangToList(anhang);
            //    App.ViewModel.ShowAlert(text, 5000);
            //}
        }
    }
}
