﻿using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class Umlage
    {
        public int UmlageId { get; set; }
        [Required]
        public Betriebskostentyp Typ { get; set; }
        [Required]
        public Umlageschluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }
        public string? Notiz { get; set; }
        public virtual HKVO? HKVO { get; set; }
        public virtual Zaehler? Zaehler { get; set; }

        public virtual List<Wohnung> Wohnungen { get; set; } = new List<Wohnung>();
        public virtual List<Betriebskostenrechnung> Betriebskostenrechnungen { get; private set; } = new List<Betriebskostenrechnung>();

        public Umlage(Betriebskostentyp typ, Umlageschluessel schluessel)
        {
            Typ = typ;
            Schluessel = schluessel;
        }
    }

    public class HKVO
    {
        public int HKVOId { get; set; }

        public double? HKVO_P7 { get; set; }
        public double? HKVO_P8 { get; set; }
        public HKVO_P9A2? HKVO_P9 { get; set; }
        public virtual Zaehler? Zaehler { get; set; } // not used? Is implemented in Umlage anyway (for nach Verbrauch)

        public string? Notiz { get; set; }
    }
}