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

using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    /// <summary>
    /// Verbindet eine offene Forderung (Soll-Buchungszeile) mit ihrer ausgleichenden
    /// Zahlung (Haben-Buchungszeile) auf demselben Buchungskonto (OPOS-Prinzip).
    ///
    /// Invariante: SollZeile.BuchungskontoId == HabenZeile.BuchungskontoId
    /// Offen = SollZeile hat keinen Ausgleich, oder Σ(Ausgleiche.HabenZeile.Betrag) &lt; SollZeile.Betrag
    /// </summary>
    public class OffenerPostenAusgleich
    {
        public Guid OffenerPostenAusgleichId { get; set; }
        [Required]
        public virtual Buchungszeile SollZeile { get; set; } = null!;
        [Required]
        public virtual Buchungszeile HabenZeile { get; set; } = null!;
    }
}
