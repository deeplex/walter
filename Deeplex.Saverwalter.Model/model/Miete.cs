﻿using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Miete
    {
        public int MieteId { get; set; }
        [Required]
        public virtual Vertrag Vertrag { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public DateOnly Zahlungsdatum { get; set; }
        [Required]
        public DateOnly BetreffenderMonat { get; set; }
        [Required]
        public double Betrag { get; set; }
        public string? Notiz { get; set; }

        public Miete(DateOnly zahlungsdatum, DateOnly betreffenderMonat, double betrag)
        {
            Zahlungsdatum = zahlungsdatum;
            BetreffenderMonat = betreffenderMonat;
            Betrag = betrag;
        }
    }
}
