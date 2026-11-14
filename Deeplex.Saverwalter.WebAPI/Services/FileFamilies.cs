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

namespace Deeplex.Saverwalter.WebAPI.Services
{
    /// <summary>Ein Objekt, dessen Dateien zu einer Familie gehören: Route und Id wie im Dateipfad.</summary>
    /// <param name="Label">Wie das Objekt in der Dateiliste heißt, z. B. „Nachtrag ab 01.01.2024“.</param>
    public sealed record FileOwner(string Route, string Id, string Label);

    /// <summary>
    /// Ein Objekt mit seinen echten Kindern — z. B. ein Vertrag mit Nachträgen und Mietminderungen.
    /// Die Seite jedes Mitglieds zeigt die Dateien der ganzen Familie. Verknüpfte, eigenständige
    /// Objekte (Kontakt, Adresse, Wohnung eines Vertrags, …) gehören nicht dazu.
    /// </summary>
    public sealed record FileFamily(IReadOnlyList<FileOwner> Members);

    /// <summary>Die Familien, an einer Stelle festgelegt; jeder Typ nennt seine Wurzel und deren Kinder.</summary>
    public static class FileFamilies
    {
        private static string Date(DateOnly date) => date.ToString("dd.MM.yyyy");

        public static FileFamily Of(Vertrag vertrag) => new(
        [
            new("vertraege", $"{vertrag.VertragId}", "Vertrag"),
            .. vertrag.Versionen.OrderBy(v => v.Beginn)
                .Select(v => new FileOwner("vertragversionen", $"{v.VertragVersionId}", $"Nachtrag ab {Date(v.Beginn)}")),
            .. vertrag.Mietminderungen.OrderBy(m => m.Beginn)
                .Select(m => new FileOwner("mietminderungen", $"{m.MietminderungId}", $"Mietminderung ab {Date(m.Beginn)}")),
        ]);

        public static FileFamily Of(VertragVersion version) => Of(version.Vertrag);
        public static FileFamily Of(Mietminderung minderung) => Of(minderung.Vertrag);

        public static FileFamily Of(Wohnung wohnung) => new(
        [
            new("wohnungen", $"{wohnung.WohnungId}", "Wohnung"),
            .. wohnung.Versionen.OrderBy(v => v.Beginn)
                .Select(v => new FileOwner("wohnungversionen", $"{v.WohnungVersionId}", $"Version ab {Date(v.Beginn)}")),
        ]);

        public static FileFamily Of(WohnungVersion version) => Of(version.Wohnung);

        public static FileFamily Of(Umlage umlage) => new(
        [
            new("umlagen", $"{umlage.UmlageId}", "Umlage"),
            .. umlage.Versionen.OrderBy(v => v.Beginn)
                .Select(v => new FileOwner("umlageversionen", $"{v.UmlageVersionId}", $"Version ab {Date(v.Beginn)}")),
        ]);

        public static FileFamily Of(UmlageVersion version) => Of(version.Umlage);

        public static FileFamily Of(Zaehler zaehler) => new(
        [
            new("zaehler", $"{zaehler.ZaehlerId}", "Zähler"),
            .. zaehler.Staende.OrderBy(s => s.Datum)
                .Select(s => new FileOwner("zaehlerstaende", $"{s.ZaehlerstandId}", $"Zählerstand {Date(s.Datum)}")),
        ]);

        public static FileFamily Of(Zaehlerstand stand) => Of(stand.Zaehler);
    }
}
