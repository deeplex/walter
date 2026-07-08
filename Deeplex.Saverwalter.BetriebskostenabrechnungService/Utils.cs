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

using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public static class Utils
    {
        public static decimal GetMietminderung(Vertrag vertrag, DateOnly abrechnungsbeginn, DateOnly abrechnungsende)
        {
            var minderungen = vertrag.Mietminderungen
                .Where(m =>
                {
                    var beginBeforeEnd = m.Beginn <= abrechnungsende;
                    var endAfterBegin = m.Ende == null || m.Ende > abrechnungsbeginn;
                    return beginBeforeEnd && endAfterBegin;
                })
                .ToList();

            return minderungen
                .Sum(m =>
                {
                    var endDate = CompareExtensions.Min(m.Ende ?? abrechnungsende, abrechnungsende);
                    var beginDate = CompareExtensions.Max(m.Beginn, abrechnungsbeginn);
                    return m.Minderung * (endDate.DayNumber - beginDate.DayNumber + 1);
                }) / (abrechnungsende.DayNumber - abrechnungsbeginn.DayNumber + 1);
        }
    }
}
