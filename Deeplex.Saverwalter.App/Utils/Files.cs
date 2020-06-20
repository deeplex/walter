using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Deeplex.Saverwalter.App.Utils
{
    public static class Files
    {
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

        public static Windows.Storage.Pickers.FileOpenPicker FilePicker(params string[] filters)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            foreach (var filter in filters)
            {
                picker.FileTypeFilter.Add(filter);
            }

            return picker;
        }

        public static async Task<Anhang> PickFile(params string[] filters)
        {
            return await ExtractFrom(await FilePicker(filters).PickSingleFileAsync());
        }

        public static async Task<ImmutableList<Anhang>> PickFiles(params string[] filters)
            => (await FilePicker(filters).PickMultipleFilesAsync())
                .Select(async f => await ExtractFrom(f))
                .Select(t => t.Result)
                .ToImmutableList();
    }
}
