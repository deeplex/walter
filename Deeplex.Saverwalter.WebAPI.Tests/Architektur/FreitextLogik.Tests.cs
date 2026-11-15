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
    /// Beschreibung, Notiz und Verwendungszweck sind nur für Menschen. Keine Logik darf sie
    /// durchsuchen, zerlegen oder vergleichen — auch nicht in Migrationen oder im Frontend.
    /// Der Test durchsucht den Quelltext nach solchen Zugriffen.
    ///
    /// Einzige Ausnahme ist die Suche, bei der der Nutzer selbst einen Text eingibt und die
    /// Liste danach filtert; sie leitet nichts ab. Diese Stellen sind unten einzeln benannt.
    /// </summary>
    public class FreitextLogikTests
    {
        private const string Felder = "(Beschreibung|Notiz|Verwendungszweck)";
        private const string FelderKlein = "(beschreibung|notiz|verwendungszweck)";

        private static readonly Regex[] CSharpMuster =
        [
            new($@"\b{Felder}\??(\.Trim\(\)|\.ToLower\(\)|\.ToUpper\(\)|\.ToLowerInvariant\(\)|\.ToUpperInvariant\(\))*\??\.(StartsWith|EndsWith|Contains|Split|IndexOf|LastIndexOf|Substring|Replace)\("),
            new($@"\b{Felder}\s*\["),
            new($@"\b{Felder}\s*(==|!=)\s*(""|\$""|[A-Za-z_][\w.]*\.{Felder}\b)"),
            new($@"(""|\$"")\s*(==|!=)\s*[\w.]*\.{Felder}\b"),
            new($@"\.Equals\([^)]*\.{Felder}\b"),
            new($@"\b{Felder}\.Equals\("),
            new($@"Regex\.\w+\([^)]*\b{Felder}\b"),
            new($@"EF\.Functions\.I?Like\([^)]*\b{Felder}\b"),
        ];

        private static readonly Regex[] SqlMuster =
        [
            new($@"\b{FelderKlein}\b\s*(NOT\s+)?(I?LIKE|SIMILAR\s+TO|~\*?|=\s*')", RegexOptions.IgnoreCase),
            new($@"(POSITION|SUBSTRING|SPLIT_PART|REGEXP_\w+|LEFT|RIGHT|STRPOS)\s*\([^)]*\b{FelderKlein}\b", RegexOptions.IgnoreCase),
        ];

        private static readonly Regex[] FrontendMuster =
        [
            new($@"\b{FelderKlein}\??(\.trim\(\)|\.toLowerCase\(\)|\.toUpperCase\(\))*\??\.(startsWith|endsWith|includes|split|indexOf|lastIndexOf|match|matchAll|replace|replaceAll|search|slice|substring)\("),
            new($@"\b{FelderKlein}\s*(===|!==|==|!=)\s*(['""`]|[A-Za-z_][\w.]*\.{FelderKlein}\b)"),
            new($@"\.(test|exec)\([^)]*\b{FelderKlein}\b"),
        ];

        /// <summary>Suche nach einem vom Nutzer eingegebenen Text (Datei, Zeileninhalt).</summary>
        private static readonly (string Datei, string Zeile)[] Nutzersuche =
        [
            ("Deeplex.Saverwalter.WebAPI/Controllers/BuchungssaetzeController.cs", "s.Beschreibung.ToLower().Contains(t) ||"),
            ("Deeplex.Saverwalter.WebAPI/Controllers/BuchungssaetzeController.cs", "(s.Notiz != null && s.Notiz.ToLower().Contains(t)) ||"),
            ("Deeplex.Saverwalter.WebAPI/Services/DbServices/ErhaltungsaufwendungDbService.cs", "p.Satz.Beschreibung.ToLower().Contains(t) ||"),
            ("Deeplex.Saverwalter.WebAPI/Services/DbServices/TransaktionDbService.cs", "e.Verwendungszweck.ToLower().Contains(t) ||"),
            ("Deeplex.Saverwalter.WebAPI/Services/DbServices/TransaktionDbService.cs", "(e.Notiz != null && e.Notiz.ToLower().Contains(t)),"),
        ];

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Deeplex.Saverwalter.sln")))
                dir = dir.Parent;
            return dir?.FullName ?? throw new InvalidOperationException("Repository-Wurzel nicht gefunden.");
        }

        private static bool IstAusgeschlossen(string relativ) =>
            relativ.Split('/').Any(teil =>
                teil is "bin" or "obj" or "node_modules" or "build" or "TestResults"
                || teil.StartsWith('.')
                || teil.EndsWith(".Tests"))
            || relativ.EndsWith(".test.ts")
            || relativ.EndsWith(".spec.ts");

        private static IEnumerable<string> Treffer(string root, string muster, Func<string, Regex[]> regeln)
        {
            var optionen = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            foreach (var datei in Directory.EnumerateFiles(root, muster, optionen))
            {
                var relativ = Path.GetRelativePath(root, datei).Replace('\\', '/');
                if (IstAusgeschlossen(relativ)) continue;

                var zeilen = File.ReadAllLines(datei);
                for (var i = 0; i < zeilen.Length; i++)
                {
                    var zeile = zeilen[i].Trim();
                    if (zeile.StartsWith("//") || zeile.StartsWith("*") || zeile.StartsWith("///")) continue;
                    if (Nutzersuche.Contains((relativ, zeile))) continue;
                    if (regeln(relativ).Any(r => r.IsMatch(zeilen[i])))
                        yield return $"{relativ}:{i + 1}: {zeile}";
                }
            }
        }

        [Fact]
        public void BackendUndMigrationenWertenKeinenFreitextAus()
        {
            var root = RepoRoot();
            var treffer = Treffer(root, "*.cs", relativ =>
                relativ.Contains("/Migrations/") ? [.. CSharpMuster, .. SqlMuster] : CSharpMuster);

            string.Join("\n", treffer).Should().BeEmpty("Beschreibung, Notiz und Verwendungszweck sind nur für Menschen");
        }

        [Fact]
        public void FrontendWertetKeinenFreitextAus()
        {
            var src = Path.Combine(RepoRoot(), "Deeplex.Saverwalter.WebAPI", "svelte", "src");
            if (!Directory.Exists(src)) return;

            var treffer = Treffer(src, "*.ts", _ => FrontendMuster)
                .Concat(Treffer(src, "*.svelte", _ => FrontendMuster));

            string.Join("\n", treffer).Should().BeEmpty("Beschreibung, Notiz und Verwendungszweck sind nur für Menschen");
        }

        [Theory]
        [InlineData("if (satz.Beschreibung.StartsWith(\"Umbuchung\"))")]
        [InlineData("var x = s.Notiz?.Contains(\"Marker\");")]
        [InlineData("satz.Beschreibung[\"Erhaltungsaufwendung: \".Length..]")]
        [InlineData("s.Beschreibung == \"Miete\"")]
        [InlineData("a.Beschreibung == b.Beschreibung")]
        [InlineData("Regex.IsMatch(t.Verwendungszweck, \"\\\\d{2}/\\\\d{4}\")")]
        [InlineData("EF.Functions.ILike(s.Beschreibung, \"%Korrektur%\")")]
        public void MusterErkennenTypischeFreitextLogik(string code)
        {
            CSharpMuster.Should().Contain(r => r.IsMatch(code));
        }

        [Theory]
        [InlineData("WHERE beschreibung LIKE 'Umbuchung%'")]
        [InlineData("SPLIT_PART(beschreibung, ':', 2)")]
        public void MusterErkennenFreitextLogikInSql(string sql)
        {
            SqlMuster.Should().Contain(r => r.IsMatch(sql));
        }

        [Theory]
        [InlineData("if (z.beschreibung.startsWith('Storno')) {")]
        [InlineData("/(\\d{2})\\/(\\d{4})/.test(row.verwendungszweck)")]
        [InlineData("x.notiz === 'erledigt'")]
        public void MusterErkennenFreitextLogikImFrontend(string code)
        {
            FrontendMuster.Should().Contain(r => r.IsMatch(code));
        }

        [Theory]
        [InlineData("Beschreibung = satz.Beschreibung;")]
        [InlineData("if (s.Notiz != null)")]
        [InlineData("satz.Beschreibung = $\"Miet-Korrektur {jahr}\";")]
        public void AnzeigeUndSchreibenSindErlaubt(string code)
        {
            CSharpMuster.Should().NotContain(r => r.IsMatch(code));
        }
    }
}
