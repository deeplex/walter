using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Erhaltungsaufwendung
    {
        public int ErhaltungsaufwendungId { get; set; }
        [Required]
        public DateTime Datum { get; set; }
        [Required]
        public Guid AusstellerId { get; set; }
        [Required]
        public string Bezeichnung { get; set; }
        [Required]
        public double Betrag { get; set; }
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        public string? Notiz { get; set; }

        public Erhaltungsaufwendung(double betrag, string bezeichnung, Guid ausstellerId, DateTime datum)
        {
            Betrag = betrag;
            Bezeichnung = bezeichnung;
            AusstellerId = ausstellerId;
            Datum = datum;
        }
    }
}
