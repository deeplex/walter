using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Deeplex.Saverwalter.WinUI3
{
    public sealed class FileService : IFileService
    {
        public string databaseRoot { get; set; }

        public FileService()
        {
            var Settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            databaseRoot = Settings.Values["root"] as string;
        }

        public async Task<string> saveFile(string filename, string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = FileSavePicker(filename, filetypes);
            var picked = await picker.PickSaveFileAsync();

            return picked?.Path;
        }

        public async Task<string> pickFile(params string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = FileOpenPicker(filetypes);
            var picked = await picker.PickSingleFileAsync();

            return picked?.Path;
        }

        public async Task<List<string>> pickFiles(params string[] ext)
        {
            var filetypes = ext.Length == 0 ? new string[] { "*" } : ext;
            var picker = FileOpenPicker(filetypes);
            var files = await picker.PickMultipleFilesAsync();

            return files.Select(f => f.Path).ToList();
        }

        public async void launchFile(Anhang a)
        {
            try
            {
                var path = a.getPath(databaseRoot);
                var file = await StorageFile.GetFileFromPathAsync(path);
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            catch (Exception e)
            {
                App.Container.GetInstance<INotificationService>().ShowAlert(e.Message);
            }
        }

        public Task<List<string>> pickFiles()
        {
            throw new NotImplementedException();
        }

        private static void getWindowHandle(object picker)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }

        public static FileSavePicker FileSavePicker(string filename, params string[] filetypes)
        {
            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = filename
            };
            picker.FileTypeChoices.Add("Datei", filetypes);

            getWindowHandle(picker);

            return picker;
        }

        private static FileOpenPicker FileOpenPicker(params string[] filetypes)
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            getWindowHandle(picker);
            if (filetypes?.Length > 0)
            {
                foreach (var filter in filetypes)
                {
                    picker.FileTypeFilter.Add(filter);
                }
            }
            else
            {
                picker.FileTypeFilter.Add("*");
            }

            return picker;
        }
    }
}
