﻿using Deeplex.Saverwalter.Model;
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

        public AppViewModel Avm { get; }

        public AsyncRelayCommand Print;
        private ErhaltungsaufwendungenPrintViewModel(AppViewModel avm, IAppImplementation impl)
        {
            Avm = avm;

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
               await Utils.Files.PrintErhaltungsaufwendungen(w, false, Jahr.Value, avm, impl, filtered);
           }, _ => true);
            Jahr.Value = DateTime.Now.Year - 1;
        }
        public ErhaltungsaufwendungenPrintViewModel(Wohnung w, AppViewModel avm, IAppImplementation impl) : this(avm, impl)
        {
            var self = this;

            Wohnungen = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w, self)
            }.ToImmutableList();
        }

        public ErhaltungsaufwendungenPrintViewModel(IPerson p, AppViewModel avm, IAppImplementation impl, params Guid[] g) : this(avm, impl)
        {
            var self = this;
            var Personen = avm.ctx.JuristischePersonen
                .ToList()
                .Where(j => j.Mitglieder.Exists(m => m.PersonId == p.PersonId))
                .ToList();

            Wohnungen = avm.ctx.Wohnungen
                .ToList()
                .Where(w =>
                    w.BesitzerId == p.PersonId ||
                    Personen.Exists(p => p.PersonId == w.BesitzerId))
                .Select(w => new ErhaltungsaufwendungenPrintEntry(w, self))
                .ToImmutableList();

            Titel = p.Bezeichnung;
            Enabled.Value = g.Length == 0;

            Zusatz = avm.ctx.JuristischePersonenMitglieder
                .Include(j => j.JuristischePerson)
                .Where(j => j.PersonId == p.PersonId && !g.Contains(j.PersonId))
                .Select(z => new ErhaltungsaufwendungenPrintViewModel(
                    z.JuristischePerson, avm, impl, g.Append(z.PersonId).ToArray()))
                .ToList()
                .Concat(avm.ctx.JuristischePersonenMitglieder
                    .Where(j => j.JuristischePerson.PersonId == p.PersonId && !g.Contains(j.PersonId))
                    .Select(z => new ErhaltungsaufwendungenPrintViewModel(
                        avm.ctx.FindPerson(z.PersonId), avm, impl, g.Append(z.PersonId).ToArray()))
                    .ToList())
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
            parent.Jahr.PropertyChanged += updateSumme;
            updateSumme(null, null);
        }

        private void updateSumme(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Summe.Value = Liste
                .Where(w => w.Enabled.Value && w.Datum.Year == parent.Jahr.Value)
                .Sum(w => w.Betrag);
            Enabled.Value = Summe.Value > 0;
        }
    }
}
