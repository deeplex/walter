using Deeplex.Saverwalter.WinUI3.Utils;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deeplex.Saverwalter.WinUI3.UserControls
{
    public sealed partial class FileDrop : UserControl
    {
        public FileDrop()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
            "MainContent",
            typeof(object),
            typeof(FileDrop),
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

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var anhaenge = items.OfType<StorageFile>()
                        .Select(async storageFile => await Files.ExtractFrom(storageFile));

                    foreach (var anhang in anhaenge)
                    {
                        var a = await anhang;
                        App.Walter.Anhaenge.Add(a);
                        if (List)
                        {
                            App.ViewModel.ListAnhang.Value.DropFile(a);
                        }
                        else
                        {
                            App.ViewModel.DetailAnhang.Value.DropFile(a);
                        }
                    }
                    if (List)
                    {
                        App.ViewModel.ListAnhang.Value.AddAnhang.Execute(null);
                    }
                    else
                    {
                        App.ViewModel.DetailAnhang.Value.AddAnhang.Execute(null);
                    }
                }
            }

            AddFilePanel.Visibility = Visibility.Collapsed;
        }
    }
}
