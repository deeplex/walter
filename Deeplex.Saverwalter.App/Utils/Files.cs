using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
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
    }
}
