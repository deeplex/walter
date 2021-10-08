using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Print;
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

        public static void SaveBetriebskostenabrechnung(this Betriebskostenabrechnung b, string path, AppViewModel avm)
        {
            var RechnungIds = b.Gruppen.SelectMany(g => g.Rechnungen).Select(r => r.BetriebskostenrechnungId);
            var temppath = Path.GetTempPath();

            var atLeastOne = false;

            avm.ctx.BetriebskostenrechnungAnhaenge
                .Include(a => a.Anhang)
                .Where(a => RechnungIds.Contains(a.Target.BetriebskostenrechnungId))
                .ToList()
                .ForEach(a =>
                {
                    if (a.Anhang != null)
                    {
                        atLeastOne = true;
                        var src = Path.Combine(avm.root, a.AnhangId.ToString() + Path.GetExtension(a.Anhang.FileName));
                        var tar = Path.Combine(temppath, a.Anhang.FileName);
                        File.Copy(src, tar);
                    }
                });

            if (atLeastOne)
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(temppath, path + ".zip");
            }
        }

        public static async Task PrintBetriebskostenabrechnung(Vertrag v, int Jahr, AppViewModel avm, IAppImplementation impl)
        {
            var b = new Betriebskostenabrechnung(avm.ctx, v.rowid, Jahr, new DateTime(Jahr, 1, 1), new DateTime(Jahr, 12, 31));

            var AuflistungMieter = string.Join(", ", avm.ctx.MieterSet
                .Where(m => m.VertragId == v.VertragId).ToList()
                .Select(a => avm.ctx.FindPerson(a.PersonId).Bezeichnung));

            var filename = Jahr.ToString() + " - " + v.Wohnung.ToString() + " - " + AuflistungMieter;
            var path = await impl.saveFile(filename, ".docx");

            b.SaveAsDocx(path);
            b.SaveBetriebskostenabrechnung(path, avm);

            impl.ShowAlert("Datei gespeichert unter " + path);
        }

        public static async Task PrintErhaltungsaufwendungen(Wohnung w, int Jahr, AppViewModel avm, IAppImplementation impl)
        {
            var filename = Jahr.ToString() + " - " + AdresseViewModel.Anschrift(w) + " " + w.Bezeichnung;
            var path = await impl.saveFile(filename, ".docx");

            var l = new ErhaltungsaufwendungWohnung(avm.ctx, w.WohnungId, Jahr);

            l.SaveAsDocx(path);
            // TODO Implement saving the Erhaltungsaufwendunganhänge.

            impl.ShowAlert("Datei gespeichert unter " + path);
        }

        public static async Task PrintErhaltungsaufwendungen(
            List<Wohnung> Wohnungen,
            bool extended,
            int Jahr,
            AppViewModel avm,
            IAppImplementation impl,
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

                var path = await impl.saveFile(filename, ".docx");

            if (path == null)
            {
                return;
            }

            var l = Wohnungen.Select(w => new ErhaltungsaufwendungWohnung(avm.ctx, w.WohnungId, Jahr)).ToImmutableList();

            if (filter is IList i && i.Count > 0)
            {
                l.ForEach(w => w.Liste = w.Liste
                 .Where(e => !filter.Contains(e.Entity))
                 .ToImmutableList());
            }
            l.SaveAsDocx(path);
            // TODO Implement saving the Erhaltungsaufwendunganhänge.

            impl.ShowAlert("Datei gespeichert unter " + path);
        }
    }
}
