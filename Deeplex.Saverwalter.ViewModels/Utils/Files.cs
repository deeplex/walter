using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels.Utils
{
    public static class Files
    {
        public static void ConnectAnhangToEntity<T, U>(DbSet<T> Set, U target, List<Anhang> files, IWalterDbService db) where T : class, IAnhang<U>, new()
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
            db.SaveWalter();
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

        public static void SaveBetriebskostenabrechnung(this Betriebskostenabrechnung b, string path, Services.IWalterDbService db)
        {
            var RechnungIds = b.Gruppen.SelectMany(g => g.Rechnungen).Select(r => r.BetriebskostenrechnungId);
            var temppath = Path.GetTempPath();

            var atLeastOne = false;

            db.ctx.BetriebskostenrechnungAnhaenge
                .Include(a => a.Anhang)
                .Where(a => RechnungIds.Contains(a.Target.BetriebskostenrechnungId))
                .ToList()
                .ForEach(a =>
                {
                    if (a.Anhang != null)
                    {
                        atLeastOne = true;
                        var src = Path.Combine(db.root, a.AnhangId.ToString() + Path.GetExtension(a.Anhang.FileName));
                        var tar = Path.Combine(temppath, a.Anhang.FileName);
                        File.Copy(src, tar);
                    }
                });

            if (atLeastOne)
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(temppath, path + ".zip");
            }
        }

        public static async Task<string> PrintBetriebskostenabrechnung(Vertrag v, int Jahr, IWalterDbService db, IFileService fs)
        {
            var b = new Betriebskostenabrechnung(db.ctx, v.rowid, Jahr, new DateTime(Jahr, 1, 1), new DateTime(Jahr, 12, 31));

            var AuflistungMieter = string.Join(", ", db.ctx.MieterSet
                .Where(m => m.VertragId == v.VertragId).ToList()
                .Select(a => db.ctx.FindPerson(a.PersonId).Bezeichnung));

            var filename = Jahr.ToString() + " - " + v.Wohnung.ToString() + " - " + AuflistungMieter;
            var path = await fs.saveFile(filename, new string[] { ".docx" });

            b.SaveAsDocx(path);
            b.SaveBetriebskostenabrechnung(path, db);

            // TODO print when called
            return path;
        }

        public static async Task<string> PrintErhaltungsaufwendungen(Wohnung w, int Jahr, IWalterDbService db, IFileService fs)
        {
            var filename = Jahr.ToString() + " - " + AdresseViewModel.Anschrift(w) + " " + w.Bezeichnung;
            var path = await fs.saveFile(filename, new string[] { ".docx" });

            var l = new ErhaltungsaufwendungWohnung(db.ctx, w.WohnungId, Jahr);

            l.SaveAsDocx(path);
            // TODO Implement saving the Erhaltungsaufwendunganhänge.

            // TODO print when called
            return path;
        }

        public static async Task PrintErhaltungsaufwendungen(
            List<Wohnung> Wohnungen,
            bool extended,
            int Jahr,
            IWalterDbService db,
            IFileService fs,
            List<Model.Erhaltungsaufwendung> filter = null)
        {
            var filename = Jahr.ToString() + " - " + Wohnungen.GetWohnungenBezeichnung();
            if (extended)
            {
                filename += " + Zusatz";
            }
            if (filter is IList e && e.Count > 0)
            {
                filename += " (anteilig)";
            }

            var path = await fs.saveFile(filename, new string[] { ".docx" });

            if (path == null)
            {
                return;
            }

            var l = Wohnungen.Select(w => new ErhaltungsaufwendungWohnung(db.ctx, w.WohnungId, Jahr)).ToImmutableList();

            if (filter is IList i && i.Count > 0)
            {
                l.ForEach(w => w.Liste = w.Liste
                 .Where(e => !filter.Contains(e.Entity))
                 .ToImmutableList());
            }
            l.SaveAsDocx(path);
            // TODO Implement saving the Erhaltungsaufwendunganhänge.

            // TODO alert when called.
            //fs.ShowAlert("Datei gespeichert unter " + path);
        }
    }
}
