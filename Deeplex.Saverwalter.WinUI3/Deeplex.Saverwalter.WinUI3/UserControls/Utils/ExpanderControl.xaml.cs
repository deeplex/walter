using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class ExpanderControl : UserControl
    {

        public void ClickWrap(object sender, RoutedEventArgs r)
        {
            if (ClickAvailable)
            {
                Click();
            }
        }

        public bool BonusAvailable => Bonus != null;
        public bool ClickAvailable => Click != null;

        public ExpanderControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
            "MainContent",
            typeof(object),
            typeof(ExpanderControl),
            new PropertyMetadata(default(object)));

        public object MainContent
        {
            get { return GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty BonusProperty =
            DependencyProperty.Register(
            "Bonus",
            typeof(object),
            typeof(ExpanderControl),
            new PropertyMetadata(default(object)));

        public object Bonus
        {
            get { return GetValue(BonusProperty); }
            set { SetValue(BonusProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty
            = DependencyProperty.Register(
                "IsExpanded",
                typeof(bool),
                typeof(ExpanderControl),
                new PropertyMetadata(false));

        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }

        public static readonly DependencyProperty ClickProperty
            = DependencyProperty.Register(
                "Click",
                typeof(Action),
                typeof(ExpanderControl),
                new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
            "Header",
            typeof(string),
            typeof(ExpanderControl),
            new PropertyMetadata(null));

        public RelayCommand Expanded
        {
            get { return (RelayCommand)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }

        public static readonly DependencyProperty ExpandedProperty
            = DependencyProperty.Register(
                "Expanded",
                typeof(RelayCommand),
                typeof(ExpanderControl),
                new PropertyMetadata(null));

        private void Expander_Expanded(object sender, EventArgs e)
        {
            if (Expanded is RelayCommand r)
            {
                r.Execute(e);
            }
        }
    }
}
