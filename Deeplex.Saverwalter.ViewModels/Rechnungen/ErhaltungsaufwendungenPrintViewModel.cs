using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class ErhaltungsaufwendungenPrintViewModel
    {
        public ObservableProperty<ImmutableList<ErhaltungsaufwendungenPrintEntry>> Wohnungen =
            new ObservableProperty<ImmutableList<ErhaltungsaufwendungenPrintEntry>>();

        public ErhaltungsaufwendungenPrintViewModel(Wohnung w)
        {
            Wohnungen.Value = new List<ErhaltungsaufwendungenPrintEntry>
            {
                new ErhaltungsaufwendungenPrintEntry(w)
            }.ToImmutableList();
        }
    }

    public sealed class ErhaltungsaufwendungenPrintEntry
    {
        public int Id { get; }
        public string Bezeichnung { get; }

        public ErhaltungsaufwendungenPrintEntry(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung = AdresseViewModel.Anschrift(w) + " - " + w.Bezeichnung;
        }
    }
}
