using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Konto
    {
        public int KontoId { get; set; }
        public string Bank { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string? Notiz { get; set; }
    }
}
