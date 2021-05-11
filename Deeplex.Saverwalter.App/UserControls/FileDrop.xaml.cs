using Deeplex.Saverwalter.App.Utils;
using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

        public static readonly DependencyProperty EntityProperty =
            DependencyProperty.Register(
            "Entity",
            typeof(object),
            typeof(FileDrop),
            new PropertyMetadata(default(object)));

        public object Entity
        {
            get { return GetValue(EntityProperty); }
            set { SetValue(EntityProperty, value); }
        }

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
                        App.ViewModel.DetailAnhang.Value.DropFile(a);
                    }

                    App.ViewModel.DetailAnhang.Value.AddAnhang.Execute(null);
                }
            }

            AddFilePanel.Visibility = Visibility.Collapsed;
        }
    }
}
