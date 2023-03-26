namespace Deeplex.Saverwalter.Model
{
    public class Miete
    {
        public int MieteId { get; set; }
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public DateTime Zahlungsdatum { get; set; }
        public DateTime BetreffenderMonat { get; set; }
        public double Betrag { get; set; }
        public string? Notiz { get; set; }

        public Miete(DateTime zahlungsdatum, DateTime betreffenderMonat, double betrag)
        {
            Zahlungsdatum = zahlungsdatum;
            BetreffenderMonat = betreffenderMonat;
            Betrag = betrag;
        }
    }
}
