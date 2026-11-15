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

using System.Text.Json.Serialization;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HinweisArt
    {
        /// <summary>Zähler, Zählerstände und Verbrauchsaufteilung.</summary>
        Verbrauch,
        /// <summary>Miet-Forderungen und Mietzahlungen.</summary>
        Miete,
        Sonstiges,
    }

    /// <summary>
    /// Hinweis des Abrechnungslaufs. Art und Sperrwirkung stehen in eigenen Feldern;
    /// der Text ist nur für Menschen und wird nicht ausgewertet.
    /// </summary>
    public sealed record Hinweis(HinweisArt Art, string Text, bool SperrtBuchung = false);
}
