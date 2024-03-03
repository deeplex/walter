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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;

namespace Deeplex.Saverwalter.PrintService
{
    public interface IPrint<T>
    {
        public T body { get; }

        public void Table(int[] widths, int[] justification, bool[] bold, bool[] underlined, string[][] cols);
        public void Text(string s);
        public void PageBreak();
        public void Break();
        public void EqHeizkostenV9_2(Betriebskostenabrechnung abrechnung, Abrechnungseinheit abrechnungseinheit);
        public void Heading(string str);
        public void SubHeading(string str);
        public void Paragraph(params PrintRun[] runs);
    }
}
