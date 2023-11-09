using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.Model
{
    public interface IErhaltungsaufwendungWohnung
    {
        ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        Wohnung Wohnung { get; }
        double Summe { get; }
    }

    public sealed class ErhaltungsaufwendungWohnung : IErhaltungsaufwendungWohnung
    {
        public ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        public Wohnung Wohnung { get; }
        public double Summe => Liste.Sum(e => e.Betrag);

        public ErhaltungsaufwendungWohnung(SaverwalterContext ctx, int WohnungId, int Jahr)
        {
            Wohnung = ctx.Wohnungen.Find(WohnungId)!;

            // TODO sort by... Aussteller, then Datum, then Bezeichnung
            Liste = ctx.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .Where(e => e.Wohnung.WohnungId == WohnungId)
                .Where(e => e.Datum.Year == Jahr)
                .Select(e => new ErhaltungsaufwendungListeEntry(e, ctx))
                .ToImmutableList();
        }
    }
}
