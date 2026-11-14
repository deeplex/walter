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
    public class Buchungszeile
    {
        public Guid BuchungszeileId { get; set; }
        [Required]
        public virtual Buchungssatz Buchungssatz { get; set; } = null!;
        [Required]
        public virtual Buchungskonto Buchungskonto { get; set; } = null!;
        [Required]
        public SollHaben SollHaben { get; set; }
        [Required]
        public decimal Betrag { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Bei einer Zahlungszeile: der offene Posten, für den sie gebucht wurde (Sollstellung,
        /// Rechnung, Abrechnung). Wird beim Buchen gesetzt und bleibt unverändert — auch wenn
        /// der Ausgleich (<see cref="AlsSollZeile"/>/<see cref="AlsHabenZeile"/>) durch ein
        /// Storno entfällt. Der Ausgleich sagt, ob der Posten bezahlt ist; die Zuordnung,
        /// wofür die Zahlung war.
        /// </summary>
        public virtual Buchungszeile? ZugeordnetZu { get; set; }

        public virtual List<OffenerPostenAusgleich> AlsSollZeile { get; private set; } = [];
        public virtual List<OffenerPostenAusgleich> AlsHabenZeile { get; private set; } = [];

        public Buchungszeile(SollHaben sollHaben, decimal betrag)
        {
            SollHaben = sollHaben;
            Betrag = betrag;
        }
    }

    public enum SollHaben
    {
        Soll,
        Haben,
    }
}
