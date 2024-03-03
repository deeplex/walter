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

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class Zeitraum
    {
        public DateOnly Nutzungsbeginn { get; }
        public DateOnly Nutzungsende { get; }
        public DateOnly Abrechnungsbeginn { get; }
        public DateOnly Abrechnungsende { get; }
        public int Abrechnungszeitraum { get; }
        public int Nutzungszeitraum { get; }
        public double Zeitanteil { get; }
        public int Jahr { get; }

        public Zeitraum(int jahr, Vertrag vertrag)
        {
            Jahr = jahr;
            Abrechnungsbeginn = new DateOnly(jahr, 1, 1);
            Abrechnungsende = new DateOnly(jahr, 12, 31);
            Nutzungsbeginn = Utils.Max(vertrag.Beginn(), Abrechnungsbeginn);
            Nutzungsende = Utils.Min(vertrag.Ende ?? Abrechnungsende, Abrechnungsende);

            Nutzungszeitraum = Nutzungsende.DayNumber - Nutzungsbeginn.DayNumber + 1;
            Abrechnungszeitraum = Abrechnungsende.DayNumber - Abrechnungsbeginn.DayNumber + 1;
            Zeitanteil = (double)Nutzungszeitraum / Abrechnungszeitraum;
        }
    }
}
