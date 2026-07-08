// Copyright (c) 2023-2026 Kai Lawrence
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

namespace Deeplex.Saverwalter.Model
{
    public static class DateUtils
    {
        /// <summary>
        /// §556b Abs. 1 BGB: Miete fällig am 3. Werktag des Monats.
        /// Samstag gilt als Werktag. Bundesweite Feiertage werden nicht berücksichtigt
        /// (bundeslandabhängig).
        /// </summary>
        public static DateOnly DritterWerktag(DateOnly monat)
        {
            var tag = new DateOnly(monat.Year, monat.Month, 1);
            int werktage = 0;
            while (werktage < 3)
            {
                if (tag.DayOfWeek != DayOfWeek.Sunday)
                    werktage++;
                if (werktage < 3)
                    tag = tag.AddDays(1);
            }
            return tag;
        }
    }
}
