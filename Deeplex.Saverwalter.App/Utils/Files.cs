using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Deeplex.Saverwalter.App.Utils
{
    public static class Files
    {
        public static async Task<Anhang> ExtractFrom(string path)
            => await ExtractFrom(await StorageFile.GetFileFromPathAsync(path));

        public static async Task<Anhang> ExtractFrom(IStorageFile stream)
        {
            var anhang = new Anhang();
            anhang.FileName = stream.Name;
            anhang.ContentType = stream.ContentType;
            anhang.CreationTime = stream.DateCreated.UtcDateTime;
            var b = await FileIO.ReadBufferAsync(stream);
            anhang.Content = b.ToArray();
            anhang.Sha256Hash = SHA256.Create().ComputeHash(anhang.Content);

            return anhang;
        }

        public static async Task<string> ExtractTo(Anhang a)
        {
            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = a.FileName,
            };

            picker.FileTypeChoices.Add("", new List<string> { Path.GetExtension(a.FileName) });

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, a.Content);
                return file.Path;
            }

            return "";
        }

        public static FileOpenPicker FilePicker(params string[] filters)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            if (filters != null && filters.Length > 0)
            {
                foreach (var filter in filters)
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
