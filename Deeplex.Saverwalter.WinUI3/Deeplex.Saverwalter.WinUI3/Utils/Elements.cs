using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.WinUI3.Utils
{
    public static class Elements
    {
        public static async void SetDatabaseAsDefault()
        {
            if (await App.Impl.Confirmation("Als Standard festlegen", "Die Datenbank: " + App.ViewModel.root + " als Standard festlegen?", "Ja", "Nein"))
            {
                var Settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Settings.Values["root"] = App.ViewModel.root;
            };
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
