using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungenPrintCommandBarControl : UserControl
    {
        public ErhaltungsaufwendungenPrintCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Erhaltungsaufwendungen"; // TODO Bezeichnung...
        }

        public ErhaltungsaufwendungenPrintViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenPrintViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(List<ErhaltungsaufwendungenListViewModel>),
            typeof(ErhaltungsaufwendungenPrintCommandBarControl),
            new PropertyMetadata(null));
    }
}
