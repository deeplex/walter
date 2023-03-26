﻿using Castle.Core.Internal;
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

        public static Adresse? GetAdresse(AdresseEntryBase adresse, SaverwalterContext ctx)
        {
            if (adresse.Strasse.IsNullOrEmpty() ||
                adresse.Hausnummer.IsNullOrEmpty() ||
                adresse.Postleitzahl.IsNullOrEmpty() ||
                adresse.Stadt.IsNullOrEmpty())
            {
                return null;
            }

            return ctx.Adressen.SingleOrDefault(e =>
                e.Strasse == adresse!.Strasse &&
                e.Hausnummer == adresse!.Hausnummer &&
                e.Postleitzahl == adresse!.Postleitzahl &&
                e.Stadt == adresse!.Stadt) ??
                new Adresse()
                {
                    Strasse = adresse.Strasse!,
                    Hausnummer = adresse.Hausnummer!,
                    Postleitzahl = adresse.Postleitzahl!,
                    Stadt = adresse.Stadt!
                };
        }
    }
}
