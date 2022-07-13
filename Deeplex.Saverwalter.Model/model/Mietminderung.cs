﻿using System;
using System.Collections.Generic;

namespace Deeplex.Saverwalter.Model
{
    // Mietminderung is later taken away from the result of the Betriebskostenabrechnug.
    public sealed class MietMinderung : IAnhang
    {
        public int MietMinderungId { get; set; }
        public Guid VertragId { get; set; }
        public DateTime Beginn { get; set; }
        public DateTime? Ende { get; set; } = null!;
        public double Minderung { get; set; }
        public string? Notiz { get; set; }
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    }
}
