using Deeplex.Saverwalter.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Deeplex.Saverwalter.App.Views
{
    public sealed partial class VertragDetailViewPage : Page
    {
        public VertragDetailViewModel ViewModel { get; set; }

        public VertragDetailViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Guid vertragId)
            {
                ViewModel = new VertragDetailViewModel(vertragId);
            }
            else // If invoked using "Add"
            {
                //ViewModel = new KontaktViewModel
                //{
                //    IsNewCustomer = true,
                //    IsInEdit = true
                //};
            }

            // ViewModel.AddNewCustomerCanceled += AddNewCustomerCanceled;
            base.OnNavigatedTo(e);
        }

        private void KontaktSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = ViewModel.Kontakte.Value
                    .Where(k => k.Name.Value.Contains(sender.Text))
                    .Select(k => k.Name.Value).ToList();

                sender.ItemsSource = suggestions.Count > 0
                    ? suggestions
                    : new List<string> { sender.Text + " (Kontakt hinzufügen)" };
            }
        }
    }
}
