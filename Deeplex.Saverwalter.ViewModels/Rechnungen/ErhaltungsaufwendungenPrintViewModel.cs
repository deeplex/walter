using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenPrintViewModel
    {
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();
        public ImmutableList<ErhaltungsaufwendungenPrintEntry> Wohnungen { get; }
        public ImmutableList<ErhaltungsaufwendungenPrintViewModel> Zusatz;
        public string Titel { get; }
        public ObservableProperty<bool> Enabled = new ObservableProperty<bool>(true);

        public IWalterDbService Db { get; }

        public AsyncRelayCommand Print;
        private ErhaltungsaufwendungenPrintViewModel(IWalterDbService db, IAppImplementation impl)
        {
            Db = db;

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

                await Utils.Files.PrintErhaltungsaufwendungen(w, false, Jahr.Value, db, impl, filtered);
            }, _ => true);
            Jahr.Value = DateTime.Now.Year - 1;
        }
        public ErhaltungsaufwendungenPrintViewModel(Wohnung w, IWalterDbService db, IAppImplementation impl) : this(db, impl)
        {
            var self = this;

            Wohnungen = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w, self)
            }.ToImmutableList();
        }

        public ErhaltungsaufwendungenPrintViewModel(IPerson p, IWalterDbService db, IAppImplementation impl, params Guid[] g) : this(db, impl)
        {
            var self = this;
            var Personen = db.ctx.JuristischePersonen
                .ToList()
                .Where(j => j.Mitglieder.Exists(m => m.PersonId == p.PersonId))
                .ToList();

            Wohnungen = db.ctx.Wohnungen
                .ToList()
                .Where(w =>
                    w.BesitzerId == p.PersonId ||
                    Personen.Exists(p => p.PersonId == w.BesitzerId))
                .Select(w => new ErhaltungsaufwendungenPrintEntry(w, self))
                .ToImmutableList();

            Titel = p.Bezeichnung;
            Enabled.Value = g.Length == 0;

            Zusatz = db.ctx.JuristischePersonenMitglieder
                .Include(j => j.JuristischePerson)
                .Where(j => j.PersonId == p.PersonId && !g.Contains(j.PersonId))
                .Select(z => new ErhaltungsaufwendungenPrintViewModel(
                    z.JuristischePerson, db, impl, g.Append(z.PersonId).ToArray()))
                .ToList()
                .Concat(db.ctx.JuristischePersonenMitglieder
                    .Where(j => j.JuristischePerson.PersonId == p.PersonId && !g.Contains(j.PersonId))
                    .Select(z => new ErhaltungsaufwendungenPrintViewModel(
                        db.ctx.FindPerson(z.PersonId), db, impl, g.Append(z.PersonId).ToArray()))
                    .ToList())
                .ToImmutableList();
        }
    }
}
