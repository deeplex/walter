using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.Model
{
    public sealed class ErhaltungsaufwendungWohnung
    {
        public ImmutableList<ErhaltungsaufwendungListeEntry> Liste { get; }
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

    public sealed class ErhaltungsaufwendungListeEntry
    {
        public Erhaltungsaufwendung Entity { get; }
        public SaverwalterContext db { get; }
        public Wohnung Wohnung => Entity.Wohnung;
        public IPerson Aussteller => db.FindPerson(Entity.AusstellerId);
        public string Bezeichnung => Entity.Bezeichnung;
        public DateTimeOffset Datum => Entity.Datum;
        public double Betrag => Entity.Betrag;
        public bool active = true;
        //public string color => active ? "

        public ErhaltungsaufwendungListeEntry(Erhaltungsaufwendung e, SaverwalterContext _db)
        {
            Entity = e;
            db = _db;
        }
    }
}
