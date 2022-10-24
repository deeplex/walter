using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed partial class FileDropControl : UserControl
    {
        public FileDropControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
            "MainContent",
            typeof(object),
            typeof(FileDropControl),
            new PropertyMetadata(default(object)));

        public object MainContent
        {
            get { return GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public bool List
        {
            get { return (bool)GetValue(ListProperty); }
            set { SetValue(ListProperty, value); }
        }

        public static readonly DependencyProperty ListProperty
            = DependencyProperty.Register(
            "List",
            typeof(bool),
            typeof(AnhangListControl),
            new PropertyMetadata(false));

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Datei hinzufügen";
                e.DragUIOverride.IsContentVisible = true;
            }

            AddFilePanel.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            AddFilePanel.Visibility = Visibility.Collapsed;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            // TODO
            //if (e.DataView.Contains(StandardDataFormats.StorageItems))
            //{
            //    var items = await e.DataView.GetStorageItemsAsync();
            //    if (items.Count > 0)
            //    {
            //        var anhaenge = items.OfType<StorageFile>()
            //            .Select(storageFile => Files.SaveAnhang(storageFile.Path, App.Walter.root)).ToList();

            //        if (List)
            //        {
            //            App.ViewModel.ListAnhang.Value.AddAnhang.Execute(anhaenge);
            //        }
            //        else
            //        {
            //            App.ViewModel.DetailAnhang.Value.AddAnhang.Execute(anhaenge);
            //        }
            //    }
            //}

            //AddFilePanel.Visibility = Visibility.Collapsed;
        }
    }
}
