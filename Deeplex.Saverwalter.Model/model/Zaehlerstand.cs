namespace Deeplex.Saverwalter.Model
{
    public class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        public virtual Zaehler Zaehler { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public DateTime Datum { get; set; }
        public double Stand { get; set; }
        public string? Notiz { get; set; }

        public Zaehlerstand(DateTime datum, double stand)
        {
            Datum = datum;
            Stand = stand;
        }
    }
}