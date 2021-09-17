using Windows.Storage.Pickers;

namespace Deeplex.Saverwalter.WinUI3.Utils
{
    public static class Files
    {
        private static void getWindowHandle(object picker)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }

        public static FileSavePicker FileSavePicker(string desc, string suggestedFilename, params string[] filetypes)
        {
            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = suggestedFilename
            };
            picker.FileTypeChoices.Add(desc, filetypes);

            getWindowHandle(picker);

            return picker;
        }

        public static FileOpenPicker FileOpenPicker(params string[] filetypes)
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
