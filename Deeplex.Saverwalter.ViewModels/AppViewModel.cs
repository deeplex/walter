using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    // TODO necessary to fake AppViewModel... have to think about a suitable design pattern...
    //public interface IAppViewModel
    //{
    //    ObservableProperty<string> Titel { get; set; }
    //    ObservableProperty<AnhangListViewModel> DetailAnhang { get; }
    //    ObservableProperty<AnhangListViewModel> ListAnhang { get; }

    //    void clearAnhang();
    //    void updateListAnhang(AnhangListViewModel list);
    //    void updateDetailAnhang(AnhangListViewModel detail);

    //    //private void updateAnhang(ObservableProperty<AnhangListViewModel> op, AnhangListViewModel a);

    //    ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries { get; set; }
    //    ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries { get; }
    //    void updateAutoSuggestEntries(string filter);

    //    string root { get; set; }
    //    SaverwalterContext ctx { get; set; }
    //    Task initializeDatabase(IAppImplementation impl);
    //    void SaveWalter();

    //    IAppImplementation Impl { get; }
    //}

    public sealed class AppViewModel : BindableBase
    {
        public ObservableProperty<string> Titel { get; set; } = new();

        public ObservableProperty<AnhangListViewModel> DetailAnhang { get; } = new();
        public ObservableProperty<AnhangListViewModel> ListAnhang { get; } = new();

        public void clearAnhang()
        {
            updateListAnhang(null);
            updateDetailAnhang(null);
        }
        public void updateListAnhang(AnhangListViewModel list) => updateAnhang(ListAnhang, list);
        public void updateDetailAnhang(AnhangListViewModel detail) => updateAnhang(DetailAnhang, detail);
        private void updateAnhang(ObservableProperty<AnhangListViewModel> op, AnhangListViewModel a)
        {
            op.Value = a;
        }

        public ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries { get; set; }
        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries { get; } = new();

        public string root { get; set; }
        public SaverwalterContext ctx { get; set; }

        public IAppImplementation Impl { get; }

        public AppViewModel(IAppImplementation impl)
        {
            Impl = impl;
            Titel.Value = "Walter";
        }

        public async Task initializeDatabase(IAppImplementation impl)
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

                AllAutoSuggestEntries = ctx.Wohnungen.Include(w => w.Adresse).Select(w => new AutoSuggestEntry(w)).ToList()
                        .Concat(ctx.NatuerlichePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.JuristischePersonen.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.Vertraege
                            .Include(w => w.Wohnung)
                            .Where(w => w.Ende == null || w.Ende < DateTime.Now)
                            .Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.ZaehlerSet.Select(w => new AutoSuggestEntry(w))).ToList()
                        .Concat(ctx.Betriebskostenrechnungen.Include(w => w.Gruppen)
                            .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestEntry(w))).ToList()
                        .Where(w => w.Bezeichnung != null).ToImmutableList();
                AutoSuggestEntries.Value = AllAutoSuggestEntries;
            }
            catch (Exception e)
            {
                Impl.ShowAlert(e.Message);
            }
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
            // TODO i18n out of viewmodels...
            Impl.ShowAlert("Gespeichert");
        }

        public void updateAutoSuggestEntries(string filter)
        {
            if (AllAutoSuggestEntries != null)
            {
                AutoSuggestEntries.Value = AllAutoSuggestEntries.Where(w => w.ToString().ToLower().Contains(filter.ToLower())).ToImmutableList();
            }
        }
    }
}
