using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Zaehlerstand
    {
        public int ZaehlerstandId { get; set; }
        [Required]
        public virtual Zaehler Zaehler { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public DateOnly Datum { get; set; }
        [Required]
        public double Stand { get; set; }
        public string? Notiz { get; set; }

        public Zaehlerstand(DateOnly datum, double stand)
        {
            Datum = datum;
            Stand = stand;
        }
    }
}