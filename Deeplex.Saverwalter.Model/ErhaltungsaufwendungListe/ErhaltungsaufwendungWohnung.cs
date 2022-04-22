using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.Model
{
    public sealed class ErhaltungsaufwendungWohnung
    {
        public ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; set; }
        public double Summe => Liste.Sum(e => e.Betrag);
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
