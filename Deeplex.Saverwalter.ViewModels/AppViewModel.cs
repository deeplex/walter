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
    public sealed class AppViewModel : BindableBase
    {
        public ObservableProperty<string> Titel { get; set; } = new ObservableProperty<string>();

        public ObservableProperty<AnhangListViewModel> DetailAnhang
            = new ObservableProperty<AnhangListViewModel>();
        public ObservableProperty<AnhangListViewModel> ListAnhang
            = new ObservableProperty<AnhangListViewModel>();

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

        public ImmutableList<AutoSuggestEntry> AllAutoSuggestEntries;
        public ObservableProperty<ImmutableList<AutoSuggestEntry>> AutoSuggestEntries
            = new ObservableProperty<ImmutableList<AutoSuggestEntry>>();

        public string root { get; set; }
        public SaverwalterContext ctx { get; set; }

        IAppImplementation Impl;

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
                            .ThenInclude(w => w.Wohnung).Select(w => new AutoSuggestEntry(w, self))).ToList()
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

    public sealed class AutoSuggestEntry
    {
        public override string ToString() => Bezeichnung;
        public string Bezeichnung;
        public string Icon;
        public object Entity;

        public AutoSuggestEntry(NatuerlichePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(JuristischePerson a)
        {
            Entity = a;
            Icon = "ContactInfo";
            Bezeichnung = a.Bezeichnung;
        }
        public AutoSuggestEntry(Wohnung a)
        {
            Entity = a;
            Icon = "Street";
            Bezeichnung = AdresseViewModel.Anschrift(a) + " - " + a.Bezeichnung;
        }
        public AutoSuggestEntry(Zaehler a)
        {
            Entity = a;
            Icon = "Clock";
            Bezeichnung = a.Kennnummer;
        }
        public AutoSuggestEntry(Vertrag a)
        {
            Entity = a;
            Icon = "Library";
            Bezeichnung = AdresseViewModel.Anschrift(a.Wohnung) + " - " + a.Wohnung.Bezeichnung;
        }
        public AutoSuggestEntry(Betriebskostenrechnung a, AppViewModel avm)
        {
            Entity = a;
            Icon = "List";
            Bezeichnung = a.Typ.ToDescriptionString() + " - " + a.GetWohnungenBezeichnung(avm);
        }
        public AutoSuggestEntry(Erhaltungsaufwendung a)
        {
            Entity = a;
            Icon = "Bullets";
            Bezeichnung = a.Bezeichnung;
        }
    }
}
