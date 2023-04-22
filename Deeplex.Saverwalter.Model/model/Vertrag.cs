using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Vertrag
    {
        public int VertragId { get; set; }
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public Guid? AnsprechpartnerId { get; set; }
        public string? Notiz { get; set; }
        public DateOnly? Ende { get; set; }

        public virtual List<VertragVersion> Versionen { get; private set; } = new List<VertragVersion>();
        public virtual List<Miete> Mieten { get; private set; } = new List<Miete>();
        public virtual List<Mietminderung> Mietminderungen { get; private set; } = new List<Mietminderung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();

        public Vertrag()
        {
        }
    }

    public class VertragVersion
    {
        public int VertragVersionId { get; set; }
        [Required]
        public int Personenzahl { get; set; }
        [Required]
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public DateOnly Beginn { get; set; }
        [Required]
        public double Grundmiete { get; set; }
        public string? Notiz { get; set; }

        public VertragVersion(DateOnly beginn, double grundmiete, int personenzahl)
        {
            Beginn = beginn;
            Grundmiete = grundmiete;
            Personenzahl = personenzahl;
        }
    }
}
