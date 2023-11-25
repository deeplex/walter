using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Konto
    {
        public int KontoId { get; set; }
        [Required]
        public string Bank { get; set; }
        [Required]
        public string Iban { get; set; }
        [Required]
        public virtual Kontakt Besitzer { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public string? Notiz { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Konto(string bank, string iban)
        {
            Bank = bank;
            Iban = iban;
        }
    }
}
