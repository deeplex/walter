using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
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

        private ErhaltungsaufwendungenPrintViewModel()
        {
            Jahr.Value = DateTime.Now.Year - 1;
        }
        public ErhaltungsaufwendungenPrintViewModel(Wohnung w) : this()
        {
            var self = this;

            Wohnungen.Value = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w, self)
            }.ToImmutableList();
        }
        public ErhaltungsaufwendungenPrintViewModel(IPerson p, SaverwalterContext ctx) : this()
        {
            var self = this;
            var Personen = ctx.JuristischePersonen.ToList()
                .Where(j => j.Mitglieder.Exists(m => m.PersonId == p.PersonId))
                .ToList();
            
            Wohnungen.Value = ctx.Wohnungen
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
        public ObservableProperty<bool> Enabled = new ObservableProperty<bool>(true);

        private ErhaltungsaufwendungenPrintViewModel parent { get; }

        public ErhaltungsaufwendungenPrintEntry(Wohnung w, ErhaltungsaufwendungenPrintViewModel vm)
        {
            Entity = w;
            parent = vm;
            Id = w.WohnungId;
            Bezeichnung = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
        }
    }
}
