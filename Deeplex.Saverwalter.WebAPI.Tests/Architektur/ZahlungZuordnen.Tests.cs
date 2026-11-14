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

using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Architektur
{
    /// <summary>
    /// Eine Zahlung wird nur über <c>TransaktionBuchungsService.Zuordnen</c> mit ihrem Posten
    /// ausgeglichen: dort entstehen Ausgleich und <c>Buchungszeile.ZugeordnetZu</c> zusammen.
    /// Ausgleiche ohne Zahlung gibt es nur an zwei Stellen: die Gegenbuchung der Verteilung
    /// beim Storno einer Abrechnung (Abrechnungslauf) und die Umbuchung einer Miet-Korrektur
    /// innerhalb ihres Satzes. Jede weitere Stelle muss hier bewusst eingeordnet werden.
    /// </summary>
    public class ZahlungZuordnenTests
    {
        private const string Buchen = "Services/Buchungen/TransaktionBuchungsService.cs";

        private static readonly Regex AusgleichAnlegen = new(@"OffenePostenAusgleiche\.Add(Range)?\(|new OffenerPostenAusgleich\b");
        private static readonly Regex ZuordnungSetzen = new(@"\.ZugeordnetZu\s*=(?!=)");

        /// <summary>Ausgleiche, die keine Zahlung betreffen.</summary>
        private static readonly string[] AusgleichOhneZahlung =
        [
            "Services/Abrechnung/AbrechnungslaufService.cs",
            "Services/Buchungen/MietKorrekturService.cs",
        ];

        private static string WebApi()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Deeplex.Saverwalter.sln")))
                dir = dir.Parent;
            return Path.Combine(dir?.FullName ?? throw new InvalidOperationException("Repository-Wurzel nicht gefunden."),
                "Deeplex.Saverwalter.WebAPI");
        }

        private static IEnumerable<(string Datei, string[] Zeilen)> Quellen()
        {
            var root = WebApi();
            var optionen = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            foreach (var datei in Directory.EnumerateFiles(root, "*.cs", optionen))
            {
                var relativ = Path.GetRelativePath(root, datei).Replace('\\', '/');
                if (relativ.Split('/').Any(t => t is "bin" or "obj" || t.StartsWith('.'))) continue;
                yield return (relativ, File.ReadAllLines(datei));
            }
        }

        private static IEnumerable<(string Datei, int Nr, string Zeile)> Treffer(Regex muster) =>
            Quellen().SelectMany(q => q.Zeilen
                .Select((z, i) => (q.Datei, Nr: i + 1, Zeile: z))
                .Where(x => !x.Zeile.TrimStart().StartsWith("//") && muster.IsMatch(x.Zeile)));

        /// <summary>Zeilennummern (1-basiert) des Rumpfs von <c>Zuordnen</c>.</summary>
        private static (int Von, int Bis) ZuordnenRumpf()
        {
            var zeilen = File.ReadAllLines(Path.Combine(WebApi(), Buchen));
            var start = Array.FindIndex(zeilen, z => Regex.IsMatch(z, @"\bvoid Zuordnen\(Buchungszeile zahlung, Buchungszeile posten\)"));
            start.Should().BeGreaterThanOrEqualTo(0, "TransaktionBuchungsService.Zuordnen muss existieren");
            var tiefe = 0;
            for (var i = start; i < zeilen.Length; i++)
            {
                tiefe += zeilen[i].Count(c => c == '{') - zeilen[i].Count(c => c == '}');
                if (i > start && tiefe == 0) return (start + 1, i + 1);
            }
            throw new InvalidOperationException("Ende von Zuordnen nicht gefunden.");
        }

        private static bool InZuordnen((string Datei, int Nr, string Zeile) t, (int Von, int Bis) rumpf) =>
            t.Datei == Buchen && t.Nr >= rumpf.Von && t.Nr <= rumpf.Bis;

        [Fact]
        public void ZahlungenWerdenNurUeberZuordnenAusgeglichen()
        {
            var rumpf = ZuordnenRumpf();
            var verboten = Treffer(AusgleichAnlegen)
                .Where(t => !InZuordnen(t, rumpf) && !AusgleichOhneZahlung.Contains(t.Datei))
                .Select(t => $"{t.Datei}:{t.Nr}: {t.Zeile.Trim()}");

            string.Join("\n", verboten).Should().BeEmpty();
        }

        [Fact]
        public void ZuordnungWirdNurInZuordnenGesetzt()
        {
            var rumpf = ZuordnenRumpf();
            var verboten = Treffer(ZuordnungSetzen)
                .Where(t => !InZuordnen(t, rumpf))
                .Select(t => $"{t.Datei}:{t.Nr}: {t.Zeile.Trim()}");

            string.Join("\n", verboten).Should().BeEmpty();
        }
    }
}
