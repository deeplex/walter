﻿using System;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Miete
    {
        public int MieteId { get; set; }
        public Guid VertragId { get; set; }
        // Zahlungsdatum may be used to determine if the last Zahlung is more than a month ago (+ tolerance).
        public DateTime Zahlungsdatum { get; set; }
        // BetreffenderMonat to be able to track single Mietsausfälle in specific months.
        public DateTime BetreffenderMonat { get; set; }
        public double? Betrag { get; set; }
        public string? Notiz { get; set; }
    }
}