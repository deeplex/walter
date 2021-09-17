using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenCommandBarControl : UserControl
    {
        public ErhaltungsaufwendungenCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Erhaltungsaufwendung"; // TODO Bezeichnung...
        }

        public ErhaltungsaufwendungenDetailViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenDetailViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ErhaltungsaufwendungenDetailViewModel),
            typeof(ErhaltungsaufwendungenCommandBarControl),
            new PropertyMetadata(null));

        private async void Delete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel.Id != 0)
            {
                await ViewModel.selfDestruct();
            }
            App.Window.AppFrame.GoBack();
        }
    }
}
