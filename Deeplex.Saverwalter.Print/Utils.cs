using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.PrintService
{
    public static class Utils
    {
        public static string Anschrift(Adresse a) => a.Strasse + " " + a.Hausnummer + ", " + a.Postleitzahl + " " + a.Stadt;

        public static string Prozent(double d) => string.Format("{0:N2}%", d * 100);
        public static string Euro(double d) => string.Format("{0:N2}€", d);
        public static string Unit(double d, string unit) => string.Format("{0:N2}" + unit, d);
        public static string Celsius(double d) => string.Format("{0:N2}°C", d);
        public static string Celsius(int d) => Celsius((double)d);
        public static string Quadrat(double d) => string.Format("{0:N2}m²", d);
        public static string Datum(DateTime d) => d.ToString("dd.MM.yyyy");
    }
}
