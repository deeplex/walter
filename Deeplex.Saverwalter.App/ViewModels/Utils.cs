using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.App.ViewModels
{
    class Utils
    {
        public static string Anschrift(Kontakt k) => Anschrift(k.Adresse);
        public static string Anschrift(Wohnung w) => Anschrift(w.Adresse);

        public static string Anschrift(Adresse a)
            => a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;
    }
}
