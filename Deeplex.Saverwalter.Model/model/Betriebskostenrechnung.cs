using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Betriebskostenrechnung
    {
        public int BetriebskostenrechnungId { get; set; }
        [Required]
        public double Betrag { get; set; }
        [Required]
        public DateOnly Datum { get; set; }
        [Required]
        public int BetreffendesJahr { get; set; }
        [Required]
        public virtual Umlage Umlage { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public string? Notiz { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Betriebskostenrechnung(double betrag, DateOnly datum, int betreffendesJahr)
        {
            Betrag = betrag;
            Datum = datum;
            BetreffendesJahr = betreffendesJahr;
        }
    }
}
