using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragListViewModelVertragVersion : BindableBase
    {
        public int Id => Entity.rowid;
        public Guid VertragId => Entity.VertragId;
        public int Version => Entity.Version;
        public int Personenzahl => Entity.Personenzahl;
        public string Anschrift => AdresseViewModel.Anschrift(Entity.Wohnung);
        public string AnschriftMitWohnung => Anschrift + ", " + Wohnung.Bezeichnung;
        public Wohnung Wohnung => Entity.Wohnung;
        public ImmutableList<Guid> Mieter => Avm.ctx.MieterSet
                .Where(w => w.VertragId == Entity.VertragId)
                .Select(m => m.PersonId).ToImmutableList();
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; }
        public string BeginnString => Beginn.ToString("dd.MM.yyyy");
        public string EndeString => Ende is DateTime e ? e.ToString("dd.MM.yyyy") : "Offen";
        public string AuflistungMieter => string.Join(", ", Avm.ctx.MieterSet
            .Where(m => m.VertragId == Entity.VertragId).ToList()
            .Select(a => Avm.ctx.FindPerson(a.PersonId).Bezeichnung));
        public bool hasEnde => Ende != null;
        public string Besitzer => Avm.ctx.FindPerson(Entity.Wohnung.BesitzerId)?.Bezeichnung;

        public Vertrag Entity { get; }
        private AppViewModel Avm;

        public VertragListViewModelVertragVersion(Vertrag v, AppViewModel avm)
        {
            Avm = avm;
            Entity = v;

            Beginn = v.Beginn.AsUtcKind();
            Ende = v.Ende?.AsUtcKind();
        }
    }
}
