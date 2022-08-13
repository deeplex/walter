using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenPrintEntry
    {
        public Wohnung Entity { get; }
        public int Id { get; }
        public string Bezeichnung { get; }
        public int Jahr => parent.Jahr;
        public ObservableProperty<bool> Enabled = new();
        public ObservableProperty<double> Summe = new();
        public ImmutableList<ErhaltungsaufwendungListViewModelEntry> Liste;

        private ErhaltungsaufwendungPrintViewModel parent { get; }

        public ErhaltungsaufwendungenPrintEntry(Wohnung w, ErhaltungsaufwendungPrintViewModel vm)
        {
            Entity = w;
            parent = vm;
            Id = w.WohnungId;
            Bezeichnung = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
            Liste = Entity.Erhaltungsaufwendungen
                .Select(e => new ErhaltungsaufwendungListViewModelEntry(e, vm.WalterDbService))
                .ToImmutableList();
            Liste.ForEach(e => e.Enabled.PropertyChanged += updateSumme);
            updateSumme(null, null);
        }

        private void updateSumme(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Summe.Value = Liste
                .Where(w => w.Enabled.Value && w.Datum.Year == parent.Jahr)
                .Sum(w => w.Betrag);
            Enabled.Value = Summe.Value > 0;
        }
    }
}
