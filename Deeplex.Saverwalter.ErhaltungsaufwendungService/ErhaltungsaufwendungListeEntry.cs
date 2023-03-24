namespace Deeplex.Saverwalter.Model
{
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
