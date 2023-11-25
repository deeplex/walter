using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Garage
    {
        public int GarageId { get; set; }
        [Required]
        public string Kennung { get; set; }
        [Required]
        public virtual Kontakt Besitzer { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public virtual Adresse? Adresse { get; set; }
        public string? Notiz { get; set; }
        public virtual List<Vertrag> Vertraege { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Garage(string kennung)
        {
            Kennung = kennung;
        }
    }
}
