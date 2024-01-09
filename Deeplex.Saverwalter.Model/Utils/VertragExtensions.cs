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

namespace Deeplex.Saverwalter.Model
{
    public static class VertragExtensions
    {
        public static DateOnly Beginn(this Vertrag v) => v.Versionen.OrderBy(e => e.Beginn).FirstOrDefault()?.Beginn ?? default;
        public static DateOnly? Ende(this VertragVersion v)
            => v.Vertrag.Versionen.OrderBy(e => e.Beginn).FirstOrDefault(e => e.Beginn > v.Beginn)?.Beginn.AddDays(-1) ??
                v.Vertrag.Ende;
    }
}
