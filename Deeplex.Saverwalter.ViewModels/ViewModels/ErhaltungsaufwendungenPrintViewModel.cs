using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenPrintViewModel : IPrintViewModel
    {
        public ObservableProperty<int> Jahr { get; } = new();
        public ImmutableList<ErhaltungsaufwendungenPrintEntry> Wohnungen { get; private set; }
        public ImmutableList<ErhaltungsaufwendungenPrintViewModel> Zusatz;
        public string Titel { get; private set; }
        public ObservableProperty<bool> Enabled = new(true);

        public IWalterDbService WalterDbService { get; }
        public IFileService FileService { get; }

        public AsyncRelayCommand Print { get; }

        public ErhaltungsaufwendungenPrintViewModel(IWalterDbService db, IFileService fs)
        {
            WalterDbService = db;
            FileService = fs;

            Print = new AsyncRelayCommand(async _ =>
            {
                var w = Wohnungen.Where(w => w.Enabled.Value)
                    .Select(w => w.Entity)
                    .ToList();
                var filtered = Wohnungen
                   .SelectMany(w => w.Liste)
                   .Where(w => !w.Enabled.Value)
                   .Select(w => w.Entity)
                   .ToList();

                var ww = Zusatz.Where(z => z.Enabled.Value)
                    .SelectMany(w => w.Wohnungen)
                    .Where(w => w.Enabled.Value)
                    .Select(w => w.Entity)
                    .ToList();
                var filterered = Zusatz.Where(z => z.Enabled.Value)
                    .SelectMany(w => w.Wohnungen)
                    .SelectMany(w => w.Liste)
                    .Where(w => !w.Enabled.Value)
                    .Select(w => w.Entity)
                    .ToList();

                w = w.Concat(ww).ToList();
                filtered = filtered.Concat(filterered).ToList();

                await Files.PrintErhaltungsaufwendungen(w, false, Jahr.Value, db, fs, filtered);
            }, _ => true);
            Jahr.Value = DateTime.Now.Year - 1;
        }
        public void SetEntity(Wohnung w)
        {
            Wohnungen = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w, this)
            }.ToImmutableList();
        }

        public void SetEntity(IPerson p, params Guid[] g)
        {
            var Personen = WalterDbService.ctx.JuristischePersonen
                .ToList()
                .Where(j => j.Mitglieder.Exists(m => m.PersonId == p.PersonId))
                .ToList();

            Wohnungen = WalterDbService.ctx.Wohnungen
                .ToList()
                .Where(w =>
                    w.BesitzerId == p.PersonId ||
                    Personen.Exists(p => p.PersonId == w.BesitzerId))
                .Select(w => new ErhaltungsaufwendungenPrintEntry(w, this))
                .ToImmutableList();

            Titel = p.Bezeichnung;
            Enabled.Value = g.Length == 0;

            Zusatz = p.JuristischePersonen // TODO check if okay
                .Where(j => j.PersonId == p.PersonId && !g.Contains(j.PersonId))
                .Select(z =>
                {
                    var vm = new ErhaltungsaufwendungenPrintViewModel(WalterDbService, FileService);
                    vm.SetEntity(z, g.Append(z.PersonId).ToArray());
                    return vm;
                })
                .ToList()
                .Concat(p.JuristischePersonen
                .Where(j => j.PersonId == p.PersonId && !g.Contains(j.PersonId))
                .Select(z =>
                {
                    var vm = new ErhaltungsaufwendungenPrintViewModel(WalterDbService, FileService);
                    vm.SetEntity(WalterDbService.ctx.FindPerson(z.PersonId), g.Append(z.PersonId).ToArray());
                    return vm;
                }))
                .ToImmutableList();
        }
    }
}
