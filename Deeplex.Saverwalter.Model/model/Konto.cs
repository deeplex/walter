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
        public string? Notiz { get; set; }

        public Konto(string bank, string iban)
        {
            Bank = bank;
            Iban = iban;
        }
    }
}
