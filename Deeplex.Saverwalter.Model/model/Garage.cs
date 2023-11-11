using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Garage
    {
        public int GarageId { get; set; }
        [Required]
        public string Kennung { get; set; }
        public virtual Adresse? Adresse { get; set; }
        public Guid BesitzerId { get; set; }
        public string? Notiz { get; set; }
        public virtual List<Vertrag> Vertraege { get; private set; } = new List<Vertrag>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Garage(string kennung)
        {
            Kennung = kennung;
        }
    }
}
