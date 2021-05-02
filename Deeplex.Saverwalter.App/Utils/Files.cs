﻿using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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

        public static async Task<StorageFolder> SelectDirectory()
        {
            var picker = new Windows.Storage.Pickers.FolderPicker()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop,
            };

            picker.FileTypeFilter.Add("*");

            return await picker.PickSingleFolderAsync(); ;
        }

        public static async Task ExtractTo(Anhang a)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop,
                SuggestedFileName = a.FileName,
            };

            picker.FileTypeChoices.Add("", new List<string> { Path.GetExtension(a.FileName) });

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, a.Content);
            }
        }

        public static Windows.Storage.Pickers.FileOpenPicker FilePicker(params string[] filters)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
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

        public static async Task<Anhang> PickFile(params string[] filters)
        {
            var file = await FilePicker(filters).PickSingleFileAsync();
            return await ExtractFrom(file);
        }

        public static async Task<List<Anhang>> PickFiles(params string[] filters)
        {
            var files = await FilePicker(filters).PickMultipleFilesAsync();
            var list = new List<Anhang>();
            foreach (var file in files)
            {
                list.Add(await ExtractFrom(file));
            }
            return list;
        }

        public static async Task SaveFilesToWalter<T, U>(Microsoft.EntityFrameworkCore.DbSet<T> Set, U target, params string[] filters) where T : class, IAnhang<U>, new()
        {
            var files = await PickFiles(filters);

            foreach (var file in files)
            {
                var attachment = new T
                {
                    Anhang = file,
                    AnhangId = file.AnhangId,
                    Target = target,
                };
                Set.Add(attachment);
                // TODO raise change in viewmodel.liste
                //App.ViewModel.Value.raiseChange(target, file);
            }
            App.SaveWalter();
        }

        public static async Task<List<T>> PrepareFilesForWalter<T>(Microsoft.EntityFrameworkCore.DbSet<T> Set, params string[] filters) where T : class, IAnhang, new()
            => (await PickFiles(filters))
                .Select(file => new T
                {
                    Anhang = file,
                    AnhangId = file.AnhangId,
                }).ToList();
    }
}
