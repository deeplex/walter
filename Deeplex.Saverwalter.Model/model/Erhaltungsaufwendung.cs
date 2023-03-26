namespace Deeplex.Saverwalter.Model
{
    public class Erhaltungsaufwendung
    {
        public int ErhaltungsaufwendungId { get; set; }
        public DateTime Datum { get; set; }
        public Guid AusstellerId { get; set; }
        public string Bezeichnung { get; set; }
        public double Betrag { get; set; }
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public string? Notiz { get; set; }

        public Erhaltungsaufwendung(double betrag, string bezeichnung, Guid ausstellerId, DateTime datum)
        {
            Betrag = betrag;
            Bezeichnung = bezeichnung;
            AusstellerId = ausstellerId;
            Datum = datum;
        }
    }
}
