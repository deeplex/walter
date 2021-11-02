﻿using Deeplex.Saverwalter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class BetriebskostenrechnungPrintCommandBarControl : UserControl
    {
        public BetriebskostenrechnungPrintCommandBarControl()
        {
            InitializeComponent();
            App.Window.CommandBar.Title = "Betriebskostenrechnung"; // TODO Bezeichnung...
        }

        public BetriebskostenrechnungPrintViewModel ViewModel
        {
            get { return (BetriebskostenrechnungPrintViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
            "ViewModel",
            typeof(BetriebskostenrechnungPrintViewModel),
            typeof(BetriebskostenrechnungPrintCommandBarControl),
            new PropertyMetadata(null));
    }
}
