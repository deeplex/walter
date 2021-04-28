using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Deeplex.Saverwalter.App.Utils
{
    public static class Elements
    {
        public static AppBarElementContainer Filter(IFilterViewModel ViewModel)

        {
            var Filter = new TextBox
            {
                Text = "",
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = 300,
                PlaceholderText = "Filter...",
            };
            Filter.TextChanged += Filter_TextChanged;
            return new AppBarElementContainer() { Content = Filter };

            void Filter_TextChanged(object sender, TextChangedEventArgs e)
            {
                ViewModel.Filter.Value = ((TextBox)sender).Text;
            }
        }
    }

    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }
}
