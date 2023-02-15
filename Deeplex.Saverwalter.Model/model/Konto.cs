using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    public class Konto : IAnhang
    {
        public int KontoId { get; set; }
        public string Bank { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string? Notiz { get; set; }
        public virtual List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
