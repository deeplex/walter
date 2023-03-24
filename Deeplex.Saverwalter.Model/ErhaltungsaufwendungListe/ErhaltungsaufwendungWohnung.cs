using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Deeplex.Saverwalter.Model
{
    public interface IErhaltungsaufwendungWohnung
    {
        ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        Wohnung Wohnung { get; }
        double Summe => Liste.Sum(e => e.Betrag);
    }

    public sealed class ErhaltungsaufwendungWohnung : IErhaltungsaufwendungWohnung
    {
        public ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        public Wohnung Wohnung { get; }

        public ErhaltungsaufwendungWohnung(SaverwalterContext _db, int WohnungId, int Jahr)
        {
            Wohnung = _db.Wohnungen.Find(WohnungId);

            // TODO sort by... Aussteller, then Datum, then Bezeichnung
            Liste = _db.Erhaltungsaufwendungen
                .Include(e => e.Wohnung)
                .Where(e => e.Wohnung.WohnungId == WohnungId)
                .Where(e => e.Datum.Year == Jahr)
                .Select(e => new ErhaltungsaufwendungListeEntry(e, _db))
                .ToImmutableList();
        }
    }
}
