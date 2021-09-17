using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Deeplex.Saverwalter.ViewModels.Utils
{
    public static class Files
    {
        public static void ConnectAnhangToEntity<T, U>(DbSet<T> Set, U target, List<Anhang> files, IAppImplementation impl, AppViewModel avm) where T : class, IAnhang<U>, new()
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
            avm.SaveWalter();
        }

        public static Anhang SaveAnhang(string src, string root)
        {
            var file = File.OpenRead(src);
            var info = new FileInfo(src);
            var anhang = new Anhang();
            anhang.FileName = Path.GetFileName(file.Name);
            anhang.ContentType = "Not implemented"; // TODO
            anhang.CreationTime = info.CreationTime;
            Directory.CreateDirectory(root);
            File.Copy(src, anhang.getPath(root));
            anhang.Sha256Hash = SHA256.Create().ComputeHash(File.Open(anhang.getPath(root), FileMode.Open));

            return anhang;
        }

        public static void SaveBetriebskostenabrechnung(this Betriebskostenabrechnung b, string path, IAppImplementation impl)
        {
            throw new NotImplementedException();
            //var RechnungIds = b.Gruppen.SelectMany(g => g.Rechnungen).Select(r => r.BetriebskostenrechnungId);
            //var temppath = Path.GetTempPath();

            //impl.ctx.BetriebskostenrechnungAnhaenge
            //    .Include(a => a.Anhang)
            //    .Where(a => RechnungIds.Contains(a.Target.BetriebskostenrechnungId))
            //    .ToList()
            //    .ForEach(a =>
            //    {
            //        if (a.Anhang != null)
            //        {
            //            var filepath = Path.Combine(temppath, a.Target.Typ.ToDescriptionString() + Path.GetExtension(a.Anhang.FileName));
            //            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            //            {
            //                fs.Write(a.Anhang.Content, 0, a.Anhang.Content.Length);
            //            }
            //        }
            //    });

            //MakeSpace(path + ".zip");
            //System.IO.Compression.ZipFile.CreateFromDirectory(temppath, path + ".zip");
        }
    }
}
