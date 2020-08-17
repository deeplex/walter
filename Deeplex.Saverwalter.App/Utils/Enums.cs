using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.Utils
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
        public Betriebskostentyp Typ { get; }
        public string Beschreibung { get; }
        public BetriebskostentypUtil(Betriebskostentyp t)
        {
            Typ = t;
            Beschreibung = t.ToDescriptionString();
        }
    }

    public sealed class HKVO9Util
    {
        public HKVO_P9A2 Enum { get; }
        public string Absatz { get; }
        public HKVO9Util(HKVO_P9A2 h)
        {
            Enum = h;
            Absatz = "Absatz " + ((int)h).ToString();
        }
    }
}
