using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE
    // A Betriebskostenrechnung may be issued to one Vertrag only, if e.g. extra costs and the Mieter is to blame.

    public class VertragsBetriebskostenrechnung
    {
        public int VertragsBetriebskostenrechnungId { get; set; }
        [Required]
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public virtual Betriebskostenrechnung Rechnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public VertragsBetriebskostenrechnung()
        {
        }
    }

}
