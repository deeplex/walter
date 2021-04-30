using Deeplex.Saverwalter.App.ViewModels;
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

namespace Deeplex.Saverwalter.App.UserControls
{
    public sealed partial class PersonDetailControl : UserControl
    {
        public PersonViewModel ViewModel { get; set; }

        public PersonDetailControl()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(PersonViewModelProperty, (DepObj, Prop) =>
            {
                ViewModel = PersonViewModel;
            });

            AddVertrag_Click = () =>
            {
                // TODO Navigate to Addvertrag with ViewModel.
            };
        }

        public PersonViewModel PersonViewModel
        {
            get { return (PersonViewModel)GetValue(PersonViewModelProperty); }
            set { SetValue(PersonViewModelProperty, value); }
        }

        public static readonly DependencyProperty PersonViewModelProperty
            = DependencyProperty.Register(
            "PersonViewModel",
            typeof(PersonViewModel),
            typeof(PersonDetailControl),
            new PropertyMetadata(null));

        public Action AddVertrag_Click;
    }
}
