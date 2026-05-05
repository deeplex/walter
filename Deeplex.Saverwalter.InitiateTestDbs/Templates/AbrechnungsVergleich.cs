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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    /// <summary>
    /// Vergleicht die alten Betriebskostenabrechnungen mit dem neuen Buchungsmodell.
    /// Gibt einen tabellarischen Bericht aus, der zeigt ob die Ergebnisse übereinstimmen.
    /// </summary>
    internal static class AbrechnungsVergleich
    {
        public static async Task ErstelleVergleichsreportAsync(SaverwalterContext ctx)
        {
            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.NkVerrechnungsKonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Wohnungen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Zaehler)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Typ)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.HKVO)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Betriebskostenrechnungen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                .Include(v => v.BkAbrechnungsKonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.NkBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.Mietminderungen)
#pragma warning disable CS0618
                .Include(v => v.Mieten)
#pragma warning restore CS0618
                .Include(v => v.Mieter)
                .ToListAsync();

            var jahre = vertraege
                .SelectMany(v => v.Wohnung.Umlagen.SelectMany(u => u.Betriebskostenrechnungen.Select(r => r.BetreffendesJahr)))
                .Distinct()
                .OrderBy(j => j)
                .ToList();

            if (jahre.Count == 0)
            {
                Console.WriteLine("Keine Betriebskostenrechnungen gefunden.");
                return;
            }

            foreach (var jahr in jahre)
                ErstelleJahresbericht(vertraege, jahr);
        }

        private static void ErstelleJahresbericht(List<Vertrag> vertraege, int jahr)
        {
            var abrechnungsbeginn = new DateOnly(jahr, 1, 1);
            var abrechnungsende = new DateOnly(jahr, 12, 31);

            Console.WriteLine();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║        Betriebskosten-Abrechnung {jahr} – Alt vs. Neu (Abweichung ≤ 0.01€)        ║");
            Console.WriteLine($"╠═══════════╦══════════════════╦══════════════════╦══════════════╦════════════╣");
            Console.WriteLine($"║ VertragId ║ Alt (Result)     ║ Neu (Saldo)      ║ Differenz    ║ Status     ║");
            Console.WriteLine($"╠═══════════╬══════════════════╬══════════════════╬══════════════╬════════════╣");

            int übereinstimmend = 0, abweichend = 0, fehler = 0;

            foreach (var vertrag in vertraege.OrderBy(v => v.VertragId))
            {
                if (!VertragAktivInJahr(vertrag, jahr)) continue;

                decimal? altResult = null;
                decimal? neuSaldo = null;
                string status;

                try
                {
                    var abrechnung = new Betriebskostenabrechnung(vertrag, jahr, abrechnungsbeginn, abrechnungsende);
                    altResult = abrechnung.Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"║ {vertrag.VertragId,-9} ║ FEHLER: {ex.Message.Truncate(60),-60} ║");
                    fehler++;
                    continue;
                }

                var bkSoll = vertrag.BkAbrechnungsKonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungssatz.Buchungsdatum.Year == jahr)
                    .Sum(z => z.Betrag);
                var bkHaben = vertrag.BkAbrechnungsKonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungssatz.Buchungsdatum.Year == jahr)
                    .Sum(z => z.Betrag);

                neuSaldo = -(bkSoll - bkHaben);

                var differenz = altResult.Value - neuSaldo.Value;
                var übereinstimmt = Math.Abs(differenz) <= 0.01m;

                if (übereinstimmt)
                {
                    status = "✓ OK";
                    übereinstimmend++;
                }
                else
                {
                    status = "✗ DIFF";
                    abweichend++;
                }

                Console.WriteLine(
                    $"║ {vertrag.VertragId,-9} ║ {altResult.Value,+15:N2}€ ║ {neuSaldo.Value,+15:N2}€ ║ {differenz,+11:N2}€ ║ {status,-10} ║");
            }

            Console.WriteLine($"╠═══════════╩══════════════════╩══════════════════╩══════════════╩════════════╣");
            Console.WriteLine($"║  Jahr: {jahr}   Übereinstimmend: {übereinstimmend,4}   Abweichend: {abweichend,4}   Fehler: {fehler,4}      ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════╝");
        }

        private static bool VertragAktivInJahr(Vertrag vertrag, int jahr)
        {
            if (!vertrag.Versionen.Any()) return false;
            var beginn = vertrag.Versionen.Min(v => v.Beginn);
            if (beginn.Year > jahr) return false;
            if (vertrag.Ende.HasValue && vertrag.Ende.Value.Year < jahr) return false;
            return true;
        }
    }

    internal static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
            => value.Length <= maxLength ? value : value[..maxLength];
    }
}
