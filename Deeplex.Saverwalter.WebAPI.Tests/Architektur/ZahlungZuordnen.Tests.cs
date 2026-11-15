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
using Deeplex.Saverwalter.Model;
using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests.Architektur
{
    /// <summary>
    /// Eine Zahlung wird nur über <see cref="Buchungszeile.Zuordnen"/> mit ihrem Posten
    /// ausgeglichen: dort entstehen Ausgleich und <see cref="Buchungszeile.ZugeordnetZu"/>
    /// zusammen, und nur dort lässt sich die Zuordnung setzen (privater Setter). Ausgleiche
    /// ohne Zahlung gibt es nur an zwei Stellen: die Gegenbuchung der Verteilung beim Storno
    /// einer Abrechnung (Abrechnungslauf) und die Umbuchung einer Miet-Korrektur innerhalb
    /// ihres Satzes. Geprüft werden alle Projekte außer Tests und Migrationen, auch
    /// SQL-Text — jede weitere Stelle muss hier bewusst eingeordnet werden.
    /// </summary>
    public class ZahlungZuordnenTests
    {
        private static readonly Regex AusgleichAnlegen =
            new(@"OffenePostenAusgleiche\.Add(Range)?\(|new OffenerPostenAusgleich\b");
        private static readonly Regex SqlAufAusgleichOderZuordnung =
            new(@"offene_posten_ausgleiche|zugeordnet_zu_id", RegexOptions.IgnoreCase);

        private static readonly string[] Erlaubt =
        [
            "Deeplex.Saverwalter.Model/model/Buchungszeile.cs",
            "Deeplex.Saverwalter.WebAPI/Services/Abrechnung/AbrechnungslaufService.cs",
            "Deeplex.Saverwalter.WebAPI/Services/Buchungen/MietKorrekturService.cs",
        ];

        private static string Wurzel()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Deeplex.Saverwalter.sln")))
                dir = dir.Parent;
            return dir?.FullName ?? throw new InvalidOperationException("Repository-Wurzel nicht gefunden.");
        }

        /// <summary>Alle C#-Quellen außer Tests, Migrationen, bin/obj und versteckten Ordnern.</summary>
        private static IEnumerable<(string Datei, string[] Zeilen)> Quellen()
        {
            var root = Wurzel();
            var optionen = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            foreach (var datei in Directory.EnumerateFiles(root, "*.cs", optionen))
            {
                var relativ = Path.GetRelativePath(root, datei).Replace('\\', '/');
                var teile = relativ.Split('/');
                if (!teile[0].StartsWith("Deeplex.Saverwalter.") || teile[0].EndsWith(".Tests")) continue;
                if (teile.Any(t => t is "bin" or "obj" or "Migrations" or "node_modules" || t.StartsWith('.'))) continue;
                yield return (relativ, File.ReadAllLines(datei));
            }
        }

        private static IEnumerable<string> Treffer(Regex muster, IEnumerable<string> ausgenommen) =>
            Quellen()
                .Where(q => !ausgenommen.Contains(q.Datei))
                .SelectMany(q => q.Zeilen
                    .Select((z, i) => (z, i))
                    .Where(x => !x.z.TrimStart().StartsWith("//") && muster.IsMatch(x.z))
                    .Select(x => $"{q.Datei}:{x.i + 1}: {x.z.Trim()}"));

        [Fact]
        public void QuellenWerdenGefunden()
        {
            Quellen().Select(q => q.Datei).Should()
                .Contain(Erlaubt)
                .And.Contain(d => d.StartsWith("Deeplex.Saverwalter.InitiateTestDbs/"));
        }

        [Fact]
        public void ZahlungenWerdenNurUeberZuordnenAusgeglichen()
        {
            string.Join("\n", Treffer(AusgleichAnlegen, Erlaubt)).Should().BeEmpty();
        }

        [Fact]
        public void KeinSqlAufAusgleicheOderZuordnung()
        {
            string.Join("\n", Treffer(SqlAufAusgleichOderZuordnung, [])).Should().BeEmpty();
        }

        [Fact]
        public void ZuordnungLaesstSichNurUeberZuordnenSetzen()
        {
            var setter = typeof(Buchungszeile).GetProperty(nameof(Buchungszeile.ZugeordnetZu))!.SetMethod;
            setter.Should().NotBeNull("EF Core braucht den Setter");
            setter!.IsPublic.Should().BeFalse();
            setter.IsAssembly.Should().BeFalse();
        }
    }
}
