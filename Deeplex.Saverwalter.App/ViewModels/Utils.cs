using Deeplex.Saverwalter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.App.ViewModels
{
    class Utils
    {
        public static string Anschrift(Kontakt k) => Anschrift(k.Adresse);
        public static string Anschrift(Wohnung w) => Anschrift(w.Adresse);

        private static string Anschrift(Adresse a)
            => a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
    }
}
