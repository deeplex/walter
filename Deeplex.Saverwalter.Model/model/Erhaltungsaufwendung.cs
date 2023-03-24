namespace Deeplex.Saverwalter.Model
{
    public class Erhaltungsaufwendung
    {
        public int ErhaltungsaufwendungId { get; set; }
        public DateTime Datum { get; set; }
        public Guid AusstellerId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public double Betrag { get; set; }
        public virtual Wohnung Wohnung { get; set; } = null!;
        public string? Notiz { get; set; }
    }
}
