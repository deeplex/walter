// Copyright (c) 2023-2025 Kai Lawrence
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
    public class Buchungskonto
    {
        public int BuchungskontoId { get; set; }
        [Required]
        public string Kontonummer { get; set; }
        [Required]
        public string Bezeichnung { get; set; }
        [Required]
        public BuchungskontoTyp Kontotyp { get; set; }
        public string? Notiz { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }
        public virtual List<Buchungszeile> Buchungszeilen { get; private set; } = new();

        public Buchungskonto(string kontonummer, string bezeichnung, BuchungskontoTyp kontotyp)
        {
            Kontonummer = kontonummer;
            Bezeichnung = bezeichnung;
            Kontotyp = kontotyp;
        }
    }

    public enum BuchungskontoTyp
    {
        Aktiv,
        Passiv,
        Aufwand,
        Ertrag,
    }
}
