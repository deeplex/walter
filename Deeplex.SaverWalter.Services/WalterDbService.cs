using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Deeplex.SaverWalter.Services
{
    public sealed class WalterDbService
    {
        public interface IWalterDbService
        {
            SaverwalterContext ctx { get; set; }
            Task initializeDatabase(IAppImplementationService impl);
            void SaveWalter();
        }

        public string root { get; set; }

        IAppImplementationService AppImplementation;
        public SaverwalterContext ctx { get; set; }


        public WalterDbService(IAppImplementationService impl)
        {
            AppImplementation = impl;
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
            // TODO this should have an own Service
            //Impl.ShowAlert("Gespeichert");
        }


        public async Task initializeDatabase(IAppImplementationService impl)
        {
            try
            {
                var self = this;

                if (root == null || !File.Exists(root + ".db"))
                {
                    var path = await impl.Confirmation(
                        "Noch keine Datenbank ausgewählt",
                        "Datenbank suchen, oder leere Datenbank erstellen?",
                        "Existierende Datenbank auswählen", "Erstelle neue leere Datenbank") ?
                        await impl.pickFile(".db") :
                        await impl.saveFile("walter", ".db");
                    root = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                }

                if (ctx != null)
                {
                    ctx.Dispose();
                }

                var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
                optionsBuilder.UseSqlite("Data Source=" + root + ".db");
                ctx = new SaverwalterContext(optionsBuilder.Options);
                ctx.Database.Migrate();

                // TODO this should be called separately in its own service
                //AllAutoSuggestEntries = ctx.Wohnungen.Include(w => w.Adresse).Select(w => new AutoSuggestEntry(w)).ToList()
                //        .Concat(ctx.NatuerlichePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                //        .Concat(ctx.JuristischePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                //        .Concat(ctx.Vertraege
                //            .Include(w => w.Wohnung)
                //            .Where(w => w.Ende == null || w.Ende < DateTime.Now)
                //            .Select(w => new AutoSuggestEntry(w))).ToList()
                //        .Concat(ctx.ZaehlerSet.Select(w => new AutoSuggestEntry(w))).ToList()
                //        .Concat(ctx.Betriebskostenrechnungen.Include(w => w.Gruppen)
                //            .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestEntry(w))).ToList()
                //        .Where(w => w.Bezeichnung != null).ToImmutableList();
                //AutoSuggestEntries.Value = AllAutoSuggestEntries;
            }
            catch (Exception e)
            {
                // TODO move to own service.
                //Impl.ShowAlert(e.Message);
            }
        }

    }
}
