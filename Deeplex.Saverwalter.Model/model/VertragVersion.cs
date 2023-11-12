using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.Model
{
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
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public VertragVersion(DateOnly beginn, double grundmiete, int personenzahl)
        {
            Beginn = beginn;
            Grundmiete = grundmiete;
            Personenzahl = personenzahl;
        }
    }
}
