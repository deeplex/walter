﻿using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    // Mietminderung is later taken away from the result of the Betriebskostenabrechnug.
    public class Mietminderung
    {
        public int MietminderungId { get; set; }
        [Required]
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public DateTime Beginn { get; set; }
        [Required]
        public double Minderung { get; set; }
        public DateTime? Ende { get; set; }
        public string? Notiz { get; set; }

        public Mietminderung(DateTime beginn, double minderung)
        {
            Beginn = beginn;
            Minderung = minderung;
        }
    }
}
