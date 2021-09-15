using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Deeplex.Saverwalter.ViewModels.Utils
{
    public static class Files
    {
        public static void SaveAnhaengeToWalter<T, U>(DbSet<T> Set, U target, List<Anhang> files, IAppImplementation impl) where T : class, IAnhang<U>, new()
        {
            foreach (var file in files)
            {
                var attachment = new T
                {
                    Anhang = file,
                    AnhangId = file.AnhangId,
                    Target = target,
                };
                Set.Add(attachment);
            }
            impl.SaveWalter();
        }

        public static void SaveBetriebskostenabrechnung(this Betriebskostenabrechnung b, string path, IAppImplementation impl)
        {
            var RechnungIds = b.Gruppen.SelectMany(g => g.Rechnungen).Select(r => r.BetriebskostenrechnungId);
            var temppath = Path.GetTempPath();

            impl.ctx.BetriebskostenrechnungAnhaenge
                .Include(a => a.Anhang)
                .Where(a => RechnungIds.Contains(a.Target.BetriebskostenrechnungId))
                .ToList()
                .ForEach(a =>
                {
                    if (a.Anhang != null)
                    {
                        var filepath = Path.Combine(temppath, a.Target.Typ.ToDescriptionString() + Path.GetExtension(a.Anhang.FileName));
                        using (FileStream fs = new FileStream(filepath, FileMode.Create))
                        {
                            fs.Write(a.Anhang.Content, 0, a.Anhang.Content.Length);
                        }
                    }
                });

            MakeSpace(path + ".zip");
            System.IO.Compression.ZipFile.CreateFromDirectory(temppath, path + ".zip");
        }

        public static bool MakeSpace(string path)
        {
            var ok = true;
            if (File.Exists(path))
            {
                var dirname = Path.GetDirectoryName(path);
                var filename = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);
                var newPath = Path.Combine(dirname, filename + ".old" + extension);
                ok = MakeSpace(newPath);
                try
                {
                    File.Move(path, newPath);
                }
                catch
                {
                    return false;
                }
            }
            return ok;
        }

        public static Anhang ExtractFrom(string path)
            => ExtractFrom(File.ReadAllBytes(path), Path.GetFileName(path), File.GetCreationTime(path));

        public static Anhang ExtractFrom(byte[] bytes, string name, DateTime creationTime)
        {
            return new Anhang()
            {
                CreationTime = creationTime,
                FileName = name,
                Content = bytes,
                Sha256Hash = SHA256.Create().ComputeHash(bytes)
            };
        }
    }
}
