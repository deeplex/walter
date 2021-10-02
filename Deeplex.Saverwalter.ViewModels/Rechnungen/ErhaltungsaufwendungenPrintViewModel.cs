using Deeplex.Saverwalter.Model;
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
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenPrintEntry>> Wohnungen =
            new ObservableProperty<ImmutableList<ErhaltungsaufwendungenPrintEntry>>();
        public ObservableProperty<int> Jahr = new ObservableProperty<int>();

        public AppViewModel Avm { get; }

        public AsyncRelayCommand Print;
        private ErhaltungsaufwendungenPrintViewModel(AppViewModel avm, IAppImplementation impl)
        {
            Avm = avm;

            Print = new AsyncRelayCommand( async _ =>
            {
                var w = Wohnungen.Value.Where(w => w.Enabled.Value)
                    .Select(w => w.Entity)
                    .ToList();
                var filtered = Wohnungen.Value
                    .SelectMany(w => w.Liste)
                    .Where(w => !w.Enabled.Value)
                    .Select(w => w.Entity)
                    .ToList();
                await Utils.Files.PrintErhaltungsaufwendungen(w, false, Jahr.Value, avm, impl, filtered);
            }, _ => true);
            Jahr.Value = DateTime.Now.Year - 1;
        }
        public ErhaltungsaufwendungenPrintViewModel(Wohnung w, AppViewModel avm, IAppImplementation impl) : this(avm, impl)
        {
            var self = this;

            Wohnungen.Value = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w, self)
            }.ToImmutableList();
        }

        public ErhaltungsaufwendungenPrintViewModel(IPerson p, AppViewModel avm, IAppImplementation impl) : this(avm, impl)
        {
            var self = this;
            var Personen = avm.ctx.JuristischePersonen
                .ToList()
                .Where(j => j.Mitglieder.Exists(m => m.PersonId == p.PersonId))
                .ToList();
            
            Wohnungen.Value = avm.ctx.Wohnungen
                .ToList()
                .Where(w =>
                    w.BesitzerId == p.PersonId ||
                    Personen.Exists(p => p.PersonId == w.BesitzerId))
                .Select(w => new ErhaltungsaufwendungenPrintEntry(w, self))
                .ToImmutableList();

        }
    }

    public sealed class ErhaltungsaufwendungenPrintEntry
    {
        public Wohnung Entity { get; }
        public int Id { get; }
        public string Bezeichnung { get; }
        public ObservableProperty<int> Jahr => parent.Jahr;
        public ObservableProperty<bool> Enabled = new ObservableProperty<bool>();
        public ObservableProperty<double> Summe = new ObservableProperty<double>();
        public ImmutableList<ErhaltungsaufwendungenListEntry> Liste;

        private ErhaltungsaufwendungenPrintViewModel parent { get; }

        public ErhaltungsaufwendungenPrintEntry(Wohnung w, ErhaltungsaufwendungenPrintViewModel vm)
        {
            Entity = w;
            parent = vm;
            Id = w.WohnungId;
            Bezeichnung = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
            Liste = Entity.Erhaltungsaufwendungen
                .Select(e => new ErhaltungsaufwendungenListEntry(e, vm.Avm))
                .ToImmutableList();
            Liste.ForEach(e => e.Enabled.PropertyChanged += updateSumme);
            updateSumme(null, null);
            Enabled.Value = Liste.Count > 0;
        }

        private void updateSumme(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Summe.Value = Liste.Where(w => w.Enabled.Value).Sum(w => w.Betrag);
        }
    }
}
