using Castle.Core.Internal;
using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Helper
{
    public static class Utils
    {
        public static string Euro(this double? d) => string.Format("{0:N2}€", d ?? 0);
        public static string Prozent(this double d) => string.Format("{0:N2}%", d);
        public static string Euro(this double d) => string.Format("{0:N2}€", d);
        public static string? Datum(this DateTime? d) => d?.ToString("dd.MM.yyyy") ?? null;
        public static string Datum(this DateTime d) => d.ToString("dd.MM.yyyy");
        public static string Zeit(this DateTime d) => d.ToString("dd.MM.yyyy HH:mm:ss");
        public static string? Zeit(this DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm:ss") ?? null;

        public static Adresse? GetAdresse(AdresseEntryBase a, SaverwalterContext ctx)
        {
            if (a.Strasse.IsNullOrEmpty() ||
                a.Hausnummer.IsNullOrEmpty() ||
                a.Postleitzahl.IsNullOrEmpty() ||
                a.Stadt.IsNullOrEmpty())
            {
                return null;
            }

            return ctx.Adressen.SingleOrDefault(e =>
                e.Strasse == a.Strasse &&
                e.Hausnummer == a.Hausnummer &&
                e.Postleitzahl == a.Postleitzahl &&
                e.Stadt == a.Stadt) ??
                new Adresse()
                {
                    Strasse = a.Strasse!,
                    Hausnummer = a.Hausnummer!,
                    Postleitzahl = a.Postleitzahl!,
                    Stadt = a.Stadt!
                };
        }
    }
}
