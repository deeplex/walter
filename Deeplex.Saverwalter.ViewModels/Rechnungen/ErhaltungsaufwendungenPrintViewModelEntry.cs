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
        public ObservableProperty<int> Jahr => parent.Jahr;
        public ObservableProperty<bool> Enabled = new ObservableProperty<bool>();
        public ObservableProperty<double> Summe = new ObservableProperty<double>();
        public ImmutableList<ErhaltungsaufwendungenListViewModelEntry> Liste;

        private ErhaltungsaufwendungenPrintViewModel parent { get; }

        public ErhaltungsaufwendungenPrintEntry(Wohnung w, ErhaltungsaufwendungenPrintViewModel vm)
        {
            Entity = w;
            parent = vm;
            Id = w.WohnungId;
            Bezeichnung = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
            Liste = Entity.Erhaltungsaufwendungen
                .Select(e => new ErhaltungsaufwendungenListViewModelEntry(e, vm.Db))
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
