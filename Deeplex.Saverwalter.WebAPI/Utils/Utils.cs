// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Helper
{
    public static class Utils
    {
        public static string Euro(this double? d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}€", d ?? 0);
        public static string Prozent(this double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}%", d);
        public static string Euro(this double d) => string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:N2}€", d);
        public static string? Datum(this DateOnly? d) => d?.ToString("dd.MM.yyyy") ?? null;
        public static string Datum(this DateOnly d) => d.ToString("dd.MM.yyyy");
        public static string Zeit(this DateTime d) => d.ToString("dd.MM.yyyy HH:mm:ss");
        public static string? Zeit(this DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm:ss") ?? null;

        public static Adresse? GetAdresse(AdresseEntryBase adresse, SaverwalterContext ctx)
        {
            if (adresse.Strasse == "" ||
                adresse.Hausnummer == "" ||
                adresse.Postleitzahl == "" ||
                adresse.Stadt == "")
            {
                return null;
            }

            return ctx.Adressen.SingleOrDefault(e =>
                e.Strasse == adresse!.Strasse &&
                e.Hausnummer == adresse!.Hausnummer &&
                e.Postleitzahl == adresse!.Postleitzahl &&
                e.Stadt == adresse!.Stadt) ??
                new Adresse(adresse.Strasse, adresse.Hausnummer, adresse.Postleitzahl, adresse.Stadt);
        }
    }
}
