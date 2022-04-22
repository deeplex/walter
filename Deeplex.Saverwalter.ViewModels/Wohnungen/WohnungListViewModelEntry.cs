using Deeplex.Saverwalter.Model;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public sealed class WohnungListViewModelEntry
    {
        public override string ToString() => Anschrift + ", " + Bezeichnung;

        public int Id { get; }
        public Wohnung Entity { get; }
        public Adresse Adresse { get; }
        public string Bezeichnung { get; }
        public string Anschrift { get; }
        public string Besitzer { get; }

        public WohnungListViewModelEntry(Wohnung w, AppViewModel avm)
        {
            Id = w.WohnungId;
            Entity = w;
            Adresse = w.Adresse;
            Bezeichnung = w.Bezeichnung;
            Anschrift = AdresseViewModel.Anschrift(w);
            Besitzer = w.BesitzerId != Guid.Empty ?
                avm.ctx.FindPerson(w.BesitzerId).Bezeichnung :
                "";
        }
    }
}
