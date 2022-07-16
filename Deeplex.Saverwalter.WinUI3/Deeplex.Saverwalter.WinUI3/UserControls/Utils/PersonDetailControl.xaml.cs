﻿using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.WinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.UserControls
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
                var vm = new VertragDetailViewModel(App.NotificationService, App.WalterService);
                if (ViewModel.isMieter.Value)
                {
                    vm.Mieter.Value = vm.Mieter.Value.Add(new KontaktListViewModelEntry(ViewModel.PersonId, App.WalterService));
                }
                else if (ViewModel.isVermieter.Value)
                {
                    vm.Wohnung.Value = vm.AlleWohnungen.First(v => v.Entity.BesitzerId == ViewModel.PersonId);
                }
                else
                {
                    App.NotificationService.ShowAlert("Person ist weder Mieter, noch Vermieter.");
                    return;
                }
                App.Window.Navigate(typeof(VertragDetailViewPage), vm);
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
