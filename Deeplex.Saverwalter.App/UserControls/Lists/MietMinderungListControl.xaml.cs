using Deeplex.Saverwalter.App.ViewModels.Mieten;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class MietMinderungListControl : UserControl
    {
        public MietMinderungListViewModel ViewModel { get; set; }

        public MietMinderungListControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(VertragGuidProperty, (DepObj, Prop) =>
            {
                ViewModel = new MietMinderungListViewModel(VertragGuid);
            });
        }

        public Guid VertragGuid
        {
            get { return (Guid)GetValue(VertragGuidProperty); }
            set { SetValue(VertragGuidProperty, value); }
        }

        public static readonly DependencyProperty VertragGuidProperty
            = DependencyProperty.Register(
                  "VertragGuid",
                  typeof(Guid),
                  typeof(VertragListControl),
                  new PropertyMetadata(Guid.Empty));
    }
}
