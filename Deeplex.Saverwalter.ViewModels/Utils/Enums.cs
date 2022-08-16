using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    class Enums
    {
        public static List<UmlageschluesselUtil> Umlageschluessel =
            Enum.GetValues(typeof(Umlageschluessel))
                .Cast<Umlageschluessel>()
                .ToList()
                .Select(t => new UmlageschluesselUtil(t))
                .ToList();

        public static List<BetriebskostentypUtil> Betriebskostentyp =
            Enum.GetValues(typeof(Betriebskostentyp))
                .Cast<Betriebskostentyp>()
                .ToList()
                .Select(e => new BetriebskostentypUtil(e))
                .ToList();

        public static List<HKVO9Util> HKVO9 =
            Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>()
                .ToList()
                .Select(t => new HKVO9Util(t))
                .ToList();

        public static List<Zaehlertyp> Zaehlertypen =
            Enum.GetValues(typeof(Zaehlertyp))
                .Cast<Zaehlertyp>()
                .ToList();

        public static List<Anrede> Anreden =
            Enum.GetValues(typeof(Anrede))
            .Cast<Anrede>()
            .ToList();
    }
}
