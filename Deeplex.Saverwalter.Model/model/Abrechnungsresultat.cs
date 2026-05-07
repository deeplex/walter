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

namespace Deeplex.Saverwalter.Model;

public class Abrechnungsresultat
{
    [Required]
    public Guid AbrechnungsresultatId { get; set; }

    [Required]
    public virtual Vertrag Vertrag { get; set; } = null!;
    [Required]
    public int Jahr { get; set; }
    [Required]
    public decimal Kaltmiete { get; set; }
    [Required]
    public decimal Vorauszahlung { get; set; }
    [Required]
    public decimal Minderung { get; set; }
    [Required]
    public decimal Rechnungsbetrag { get; set; }
    public bool Abgesendet { get; set; }
    // Positiv = Mieter muss zahlen, Negativ = Vermieter muss zahlen
    public decimal Saldo { get; set; }
    public string? Notiz { get; set; }
    /// <summary>
    /// Buchungssatz für die NK-Abrechnung. Wird beim erstmaligen Generieren angelegt.
    /// Soll NkBuchungskonto / Haben BkAbrechnungsKonto + ZahlungsKonto (Saldo).
    /// </summary>
    [Required]
    public virtual Buchungssatz Buchungssatz { get; set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModified { get; set; }
}
