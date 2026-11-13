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
    /// Buchungssätze werden nur an einer Stelle gelöscht oder storniert:
    /// <c>RueckgaengigService</c>, der die gemeinsame Regel (<c>BuchungssatzSchutzService</c>)
    /// anwendet. Einzige Ausnahme ist der Abrechnungslauf, der eine Abrechnungsgruppe als
    /// Ganzes zurücknimmt oder storniert — angestoßen ebenfalls vom <c>RueckgaengigService</c>.
    /// </summary>
    public class BuchungenEntfernenTests
    {
        private static readonly Regex DirektesEntfernen =
            new(@"Buchungs(saetze|zeilen)\.(Remove|RemoveRange)\(|\.Buchungszeilen\.(Remove|Clear)\(");
        private static readonly Regex StornoBaustein = new(@"\bStornoBuchungsService\b");

        private static readonly string[] Erlaubt =
        [
            "Services/Buchungen/RueckgaengigService.cs",
            "Services/Abrechnung/AbrechnungslaufService.cs",
        ];

        /// <summary>Definition und Registrierung des Storno-Bausteins.</summary>
        private static readonly string[] StornoBausteinSelbst =
        [
            "Services/Buchungen/StornoBuchungsService.cs",
            "Program.cs",
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

        private static IEnumerable<string> Treffer(Regex muster, IEnumerable<string> ausgenommen) =>
            Quellen()
                .Where(q => !ausgenommen.Contains(q.Datei))
                .SelectMany(q => q.Zeilen
                    .Select((z, i) => (z, i))
                    .Where(x => !x.z.TrimStart().StartsWith("//") && muster.IsMatch(x.z))
                    .Select(x => $"{q.Datei}:{x.i + 1}: {x.z.Trim()}"));

        [Fact]
        public void NurRueckgaengigEntferntBuchungen()
        {
            string.Join("\n", Treffer(DirektesEntfernen, Erlaubt)).Should().BeEmpty();
        }

        [Fact]
        public void NurRueckgaengigStorniertBuchungen()
        {
            string.Join("\n", Treffer(StornoBaustein, [.. Erlaubt, .. StornoBausteinSelbst])).Should().BeEmpty();
        }
    }
}
