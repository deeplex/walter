using CommunityToolkit.WinUI.UI.Controls;
using Deeplex.Saverwalter.ViewModels;
using Deeplex.Utils.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Utils
{
    public static class Elements
    {
        public static AppBarElementContainer CheckBox(ObservableProperty<bool> check, string label)
        {
            void Checkbox_Click(object sender, RoutedEventArgs e)
            {
                check.Value = ((CheckBox)sender).IsChecked ?? false;
            }

            var checkbox = new CheckBox()
            {
                IsChecked = check.Value,
                Content = label,
            };
            checkbox.Click += Checkbox_Click;
            return new AppBarElementContainer() { Content = checkbox };
        }

        public static AppBarElementContainer Filter(IFilterViewModel ViewModel)
        {
            var Filter = new TextBox
            {
                Text = "",
                Height = 40, // Height of Bar at the top...
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

        public static bool applyFilter(string filter, params string[] strings)
            => filter.Split(' ').All(split => strings.Any(str => str != null && str.ToLower().Contains(split.ToLower())));

        public static ImmutableList<T> Sort<T>(this DataGrid dataGrid, DataGridColumn columnToSort, ImmutableList<T> list)
        {
            var lastSortedColumn = dataGrid.Columns.Where(column =>
                column.SortDirection.HasValue).FirstOrDefault();
            bool isSortColumnDifferentThanLast = columnToSort != lastSortedColumn;
            bool isAscending = isSortColumnDifferentThanLast ||
                columnToSort.SortDirection == DataGridSortDirection.Descending;

            columnToSort.SortDirection = isAscending ?
                DataGridSortDirection.Ascending : DataGridSortDirection.Descending;
            if (isSortColumnDifferentThanLast && lastSortedColumn != null)
            {
                lastSortedColumn.SortDirection = null;
            }

            var propertyName = columnToSort.ClipboardContentBinding.Path.Path;
            object sortFunc(T obj) => obj.GetType().GetProperty(propertyName).GetValue(obj);

            var list2 = isAscending ?
                list.OrderBy(sortFunc).ToImmutableList() :
                list.OrderByDescending(sortFunc).ToImmutableList();

            return list2;
        }
    }
}
