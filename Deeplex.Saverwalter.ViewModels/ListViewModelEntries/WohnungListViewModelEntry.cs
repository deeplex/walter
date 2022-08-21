using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
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
        public IPerson Besitzer { get; }

        public WohnungListViewModelEntry(Wohnung w, IWalterDbService db)
        {
            Entity = w;
            Id = w.WohnungId;
            Adresse = w.Adresse;
            Bezeichnung = w.Bezeichnung;
            Anschrift = AdresseViewModel.Anschrift(w);
            Besitzer = w.BesitzerId != Guid.Empty ? db.ctx.FindPerson(w.BesitzerId) : null;
        }
    }
}
