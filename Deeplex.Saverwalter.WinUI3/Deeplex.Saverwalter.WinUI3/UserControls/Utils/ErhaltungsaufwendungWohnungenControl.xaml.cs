using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ErhaltungsaufwendungWohnungenControl : UserControl
    {

        public ErhaltungsaufwendungWohnungenControl()
        {
            InitializeComponent();
        }

        public ErhaltungsaufwendungenPrintViewModel ViewModel
        {
            get { return (ErhaltungsaufwendungenPrintViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(ErhaltungsaufwendungenPrintViewModel),
            typeof(ErhaltungsaufwendungWohnungenControl),
            new PropertyMetadata(null));

    }
}
