using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Vertrag
    {
        public int VertragId { get; set; }
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public virtual Kontakt? Ansprechpartner { get; set; }
        public string? Notiz { get; set; }
        public DateOnly? Ende { get; set; }

        public virtual List<VertragVersion> Versionen { get; private set; } = new();
        public virtual List<Miete> Mieten { get; private set; } = new();
        public virtual List<Mietminderung> Mietminderungen { get; private set; } = new();
        public virtual List<Garage> Garagen { get; private set; } = new();
        public virtual List<Kontakt> Mieter { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Vertrag()
        {
        }
    }
}
