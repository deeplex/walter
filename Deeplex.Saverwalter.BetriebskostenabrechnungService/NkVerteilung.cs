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
    /// <summary>
    /// Verteil-Zeilen, die der Abrechnungslauf an eine BK-Rechnung anfügt (Soll auf dem
    /// NkBuchungskonto eines Vertrags bzw. dem AufwandsKonto einer Wohnung; Gutschriften
    /// im Haben).
    ///
    /// Beim Storno einer Abrechnung bekommt jede Verteil-Zeile eine Gegenbuchung, mit der
    /// sie ausgeglichen wird. Eine Verteil-Zeile mit Ausgleich ist damit storniert; nur
    /// Zeilen ohne Ausgleich gelten als gebucht. Andere Ausgleiche an Verteil-Zeilen
    /// entstehen nicht.
    /// </summary>
    public static class NkVerteilung
    {
        public static bool IstAktiv(Buchungszeile zeile) =>
            zeile.SollHaben == SollHaben.Soll
                ? zeile.AlsSollZeile.Count == 0
                : zeile.AlsHabenZeile.Count == 0;
    }
}
