using ABI.Windows.Storage;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Deeplex.Saverwalter.Print;
using System;
using System.IO;
using System.Linq;

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

        private async void Erhaltungsaufwendung_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var Jahr = (int)((Button)sender).CommandParameter;
                var l = new ErhaltungsaufwendungWohnung(App.ViewModel.ctx, ViewModel.Id, Jahr);

                var picker = Utils.Files.FileSavePicker(Path.GetExtension(".docx"));
                picker.SuggestedFileName = Jahr.ToString() + " - " + ViewModel.Anschrift;
                var file = await picker.PickSaveFileAsync();
                var path = Path.Combine(Path.GetDirectoryName(file.Path), Path.GetFileNameWithoutExtension(file.Path));

                l.SaveAsDocx(path + ".docx");
                // TODO Implement saving the Erhaltungsaufwendunganhänge.

                App.Impl.ShowAlert("Datei gespeichert unter " + path);
            }
            catch (Exception ex)
            {
                App.Impl.ShowAlert(ex.Message);
            }
        }
    }
}
