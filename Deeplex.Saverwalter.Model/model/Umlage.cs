﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Deeplex.Saverwalter.Model
{
    public sealed class Umlage : IAnhang
    {
        public int UmlageId { get; set; }
        public Betriebskostentyp Typ { get; set; }
        public UmlageSchluessel Schluessel { get; set; }
        public string? Beschreibung { get; set; }
        public List<Anhang> Anhaenge { get; set; } = new List<Anhang>();
        public HKVO? HKVO { get; set; }
        public string? Notiz { get; set; }

        public List<Wohnung> Wohnungen { get; private set; } = new List<Wohnung>();
    }

    public sealed class HKVO
    {
        public int HKVOId { get; set; }

        public double? HKVO_P7 { get; set; }
        public double? HKVO_P8 { get; set; }
        public HKVO_P9A2? HKVO_P9 { get; set; }
        public Zaehler? Zaehler { get; set; }

        public string? Notiz { get; set; }
    }
}
