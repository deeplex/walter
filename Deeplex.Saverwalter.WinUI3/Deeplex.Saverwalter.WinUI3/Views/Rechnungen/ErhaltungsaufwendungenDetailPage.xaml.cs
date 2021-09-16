﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Saverwalter.ViewModels.Rechnungen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.WinUI3.Views.Rechnungen
{
    public sealed partial class ErhaltungsaufwendungenDetailPage : Page
    {
        public ErhaltungsaufwendungenDetailViewModel ViewModel { get; set; }

        public ErhaltungsaufwendungenDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int id)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(App.Walter.Erhaltungsaufwendungen.Find(id), App.ViewModel);
            }
            else if (e.Parameter is ErhaltungsaufwendungenDetailViewModel vm)
            {
                ViewModel = vm;
            }
            else if (e.Parameter is Erhaltungsaufwendung r)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(r, App.ViewModel);
            }
            else if (e.Parameter is null)
            {
                ViewModel = new ErhaltungsaufwendungenDetailViewModel(App.ViewModel);
            }

            App.ViewModel.Titel.Value = "Erhaltungsaufwendung";
            App.ViewModel.updateDetailAnhang(new AnhangListViewModel(ViewModel.Entity, App.Impl, App.ViewModel));

            var Delete = new AppBarButton
            {
                Label = "Löschen",
                Icon = new SymbolIcon(Symbol.Delete),
            };
            Delete.Click += SelfDestruct;

            App.Window.RefillCommandContainer(new ICommandBarElement[]
{
                new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Attach),
                    Command = ViewModel.AttachFile,
                }
            }, new ICommandBarElement[] { Delete });
            base.OnNavigatedTo(e);
        }

        private async void SelfDestruct(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (await App.Impl.Confirmation())
            {
                if (ViewModel.Id != 0)
                {
                    ViewModel.selfDestruct();
                }
                Frame.GoBack();
            }
        }

        private void AddQuickPerson_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.QuickPerson.Value != "" && ViewModel.QuickPerson.Value != null)
            {
                var j = new Model.JuristischePerson()
                {
                    Bezeichnung = ViewModel.QuickPerson.Value,
                    isHandwerker = true,
                };
                App.Walter.JuristischePersonen.Add(j);
                App.SaveWalter();

                var kl = new KontaktListEntry(j);
                ViewModel.Personen.Value = ViewModel.Personen.Value.Add(kl);
                ViewModel.Aussteller = kl;
                ViewModel.QuickPerson.Value = "";

                if (QuickPersonFlyout.Flyout is Flyout f)
                {
                    f.Hide();
                }
            }
        }
    }
}
