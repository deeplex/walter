using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    class Enums
    {
        public static List<UmlageSchluesselUtil> UmlageSchluessel =
            Enum.GetValues(typeof(UmlageSchluessel))
                .Cast<UmlageSchluessel>()
                .ToList()
                .Select(t => new UmlageSchluesselUtil(t))
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

    public sealed class UmlageSchluesselUtil
    {
        public UmlageSchluessel Schluessel { get; }
        public string Beschreibung { get; }
        public UmlageSchluesselUtil(UmlageSchluessel u)
        {
            Schluessel = u;
            Beschreibung = u.ToDescriptionString();
        }
    }

    public sealed class BetriebskostentypUtil
    {
        public override string ToString()
        {
            // TODO i18n out of viewmodel...
            return Typ.ToDescriptionString() + ((int)Typ % 2 == 0 ? " (kalt)" : " (warm)");
        }

        public Betriebskostentyp Typ { get; }
        public int index { get; }
        public BetriebskostentypUtil(Betriebskostentyp t)
        {
            Typ = t;
            index = (int)t;
        }
    }

    public sealed class HKVO9Util
    {
        public HKVO_P9A2 Enum { get; }
        public int index { get; }
        public string Absatz { get; }
        public HKVO9Util(HKVO_P9A2 h)
        {
            Enum = h;
            index = (int)h;
            // TODO i18n out of viewmodel...
            Absatz = "Absatz " + index.ToString();
        }
    }
}
