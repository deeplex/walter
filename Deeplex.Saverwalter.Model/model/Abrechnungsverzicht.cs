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

namespace Deeplex.Saverwalter.Model;

/// <summary>
/// Dokumentierter Verzicht auf die Betriebskostenabrechnung eines Vertrags für
/// ein Abrechnungsjahr (z.B. Bestandsübernahme, Zeitraum vor Programmeinführung,
/// kein Vorschuss vereinnahmt, Verjährung). Bewusst OHNE Buchungssatz — es ist
/// die Dokumentation einer geschäftlichen Entscheidung, kein Buchungsvorgang.
/// Die Jahresabschlusskontrolle behandelt einen so markierten Vertrag als erledigt.
/// </summary>
public class Abrechnungsverzicht
{
    [Required]
    public Guid AbrechnungsverzichtId { get; set; }

    [Required]
    public virtual Vertrag Vertrag { get; set; } = null!;

    /// <summary>Abrechnungsjahr, für das nicht abgerechnet wird.</summary>
    public int Jahr { get; set; }

    /// <summary>Pflicht — der Beleg für die Entscheidung.</summary>
    [Required]
    public string Grund { get; set; } = "";

    /// <summary>Datum, zu dem der Verzicht festgehalten wurde.</summary>
    public DateOnly Datum { get; set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime LastModified { get; set; }
}
