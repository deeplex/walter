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
    /// Doppelter Buchungssatz nach GoB / § 239 HGB.
    /// Korrekturen nur per Stornobuchung (StornoVon gesetzt).
    /// </summary>
    public class Buchungssatz
    {
        public Guid BuchungssatzId { get; set; }
        [Required]
        public DateOnly Buchungsdatum { get; set; }
        [Required]
        public string Beschreibung { get; set; } = string.Empty;

        /// <summary>
        /// Lückenlose fortlaufende Nummer innerhalb des Buchungsjahres (§ 239 HGB).
        /// Buchungsnummer wird beim Speichern automatisch aus der DB-Sequence zugewiesen.
        /// </summary>
        public int Buchungsnummer { get; private set; }

        /// <summary>
        /// Wirtschaftsjahr, für das dieser Satz gilt. Kann vom Buchungsdatum abweichen,
        /// </summary>
        public int Buchungsjahr { get; set; }

        /// <summary>S3-Pfad zum Originalbeleg.</summary>
        public string? Belegpfad { get; set; }

        /// <summary>
        /// Gesetzt wenn dieser Satz eine Stornobuchung ist.
        /// Der Originalsatz ist dann über StornoVon.StornoNach erreichbar.
        /// </summary>
        public virtual Buchungssatz? StornoVon { get; set; }

        /// <summary>Rückwärtsnavigation: gesetzt wenn dieser Satz bereits storniert wurde.</summary>
        public virtual Buchungssatz? StornoNach { get; private set; }

        /// <summary>Optionaler Verweis auf den importierten Kontoauszugseintrag.</summary>
        public virtual Transaktion? Transaktion { get; set; }

        public string? Notiz { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public virtual List<Buchungszeile> Buchungszeilen { get; private set; } = new();

        public Buchungssatz(DateOnly buchungsdatum, string beschreibung)
        {
            Buchungsdatum = buchungsdatum;
            Beschreibung = beschreibung;
            Buchungsjahr = buchungsdatum.Year;
        }
    }
}
