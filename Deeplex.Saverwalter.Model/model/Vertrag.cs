﻿using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Vertrag
    {
        public int VertragId { get; set; }
        [Required]
        public virtual Wohnung Wohnung { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        public Guid? AnsprechpartnerId { get; set; }
        public string? Notiz { get; set; }
        public DateOnly? Ende { get; set; }

        public virtual List<VertragVersion> Versionen { get; private set; } = new List<VertragVersion>();
        public virtual List<Miete> Mieten { get; private set; } = new List<Miete>();
        public virtual List<Mietminderung> Mietminderungen { get; private set; } = new List<Mietminderung>();
        public virtual List<Garage> Garagen { get; private set; } = new List<Garage>();
        public virtual List<Mieter> Mieter { get; private set; } = new List<Mieter>();
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public Vertrag()
        {
        }
    }
}
