using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Umlagetyp
    {
        public int UmlagetypId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }

        [Required]
        public string? Notiz { get; set; }

        public virtual List<Umlage> Umlagen { get; private set; } = new List<Umlage>();

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public Umlagetyp(string bezeichnung)
        {
            Bezeichnung = bezeichnung;
        }
    }
}

