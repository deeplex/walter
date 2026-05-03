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
    /// Erstellt Buchungssätze für die Jahres-BK-Abrechnung nach dem neuen Buchungsmodell.
    /// Zwei Schritte pro Vertrag/Jahr:
    ///   1. Dem BK-Eingang-Buchungssatz jeder Betriebskostenrechnung das anteilige Soll auf NkBuchungskonto hinzufügen
    ///   2. NkBuchungskonto-Saldo gegen BkAbrechnungsKonto ausgleichen
    /// </summary>
    internal static class BkAbrechnungNeu
    {
        public static async Task BucheJahresabrechnungAsync(SaverwalterContext ctx, int jahr)
        {
            var abrechnungsTag = new DateOnly(jahr, 12, 31);

            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.NkVerrechnungsKonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Betriebskostenrechnungen).ThenInclude(r => r.Buchungssatz)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Wohnungen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Zaehler)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Typ)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.HKVO)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Betriebskostenrechnungen)
                .Include(v => v.NkBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.BkAbrechnungsKonto)
                .Include(v => v.Mietminderungen)
                .Include(v => v.Mieter)
                .ToListAsync();

            Console.WriteLine($"Buche BK-Jahresabrechnung {jahr} für {vertraege.Count} Verträge...");

            foreach (var vertrag in vertraege)
            {
                if (!VertragAktivInJahr(vertrag, jahr)) continue;

                var notes = new List<Note>();
                var zeitraum = new Zeitraum(jahr, vertrag);
                var einheiten = Abrechnungseinheit.GetAbrechnungseinheiten(vertrag, zeitraum, notes);

                // Step 1: Add the proportional Soll to the existing BK-Eingang Buchungssatz.
                // Each Betriebskostenrechnung already has its half-open Buchungssatz (Haben side only).
                // We complete it by adding a Soll zeile on the Vertrag's NkBuchungskonto.
                foreach (var einheit in einheiten)
                {
                    foreach (var umlage in einheit.Rechnungen.Keys)
                    {
                        var anteil = einheit.GetAnteil(umlage);
                        if (anteil <= 0) continue;

                        foreach (var entry in einheit.Rechnungen[umlage])
                        {
                            if (entry.Rechnung is null || entry.Betrag <= 0) continue;

                            var betrag = Math.Round(entry.Betrag * anteil, 2);
                            if (betrag <= 0) continue;

                            var satz = entry.Rechnung.Buchungssatz;
                            AddZeile(satz, SollHaben.Soll, betrag, vertrag.NkBuchungskonto);
                        }
                    }
                }

                // Step 2: Settle NkBuchungskonto balance against BkAbrechnungsKonto.
                // NkBuchungskonto.Buchungszeilen already includes the step 1 entries added
                // above (via EF Core relationship fix-up), so the saldo is up to date.
                var nkSaldo = NkBuchungskontoSaldoImJahr(vertrag.NkBuchungskonto, jahr);

                if (nkSaldo == 0) continue;

                var ausgleichSatz = new Buchungssatz(
                    abrechnungsTag,
                    $"BK-Abrechnung {jahr} Ausgleich – {vertrag.VertragId}");

                if (nkSaldo > 0)
                {
                    // NkBuchungskonto hat Soll-Überschuss: Mieter hat Nachzahlung
                    AddZeile(ausgleichSatz, SollHaben.Haben, nkSaldo, vertrag.NkBuchungskonto);
                    AddZeile(ausgleichSatz, SollHaben.Soll, nkSaldo, vertrag.BkAbrechnungsKonto);
                }
                else
                {
                    // NkBuchungskonto hat Haben-Überschuss: Mieter bekommt Erstattung
                    var betrag = -nkSaldo;
                    AddZeile(ausgleichSatz, SollHaben.Soll, betrag, vertrag.NkBuchungskonto);
                    AddZeile(ausgleichSatz, SollHaben.Haben, betrag, vertrag.BkAbrechnungsKonto);
                }

                ctx.Buchungssaetze.Add(ausgleichSatz);
            }

            await ctx.SaveChangesAsync();
            Console.WriteLine($"BK-Jahresabrechnung {jahr} abgeschlossen.");
        }

        private static decimal NkBuchungskontoSaldoImJahr(Buchungskonto konto, int jahr)
        {
            // Soll-Buchungen = Belastungen (Kosten auf Mieter verteilt)
            // Haben-Buchungen = Gutschriften (Vorauszahlungen des Mieters)
            var soll = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungssatz.Buchungsdatum.Year == jahr)
                .Sum(z => z.Betrag);
            var haben = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungssatz.Buchungsdatum.Year == jahr)
                .Sum(z => z.Betrag);
            // Positive saldo = Nachzahlung; negative = Erstattung
            return soll - haben;
        }

        private static bool VertragAktivInJahr(Vertrag vertrag, int jahr)
        {
            if (!vertrag.Versionen.Any()) return false;
            var beginn = vertrag.Versionen.Min(v => v.Beginn);
            if (beginn.Year > jahr) return false;
            if (vertrag.Ende.HasValue && vertrag.Ende.Value.Year < jahr) return false;
            return true;
        }

        private static void AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            var zeile = new Buchungszeile(sollHaben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = konto
            };
            satz.Buchungszeilen.Add(zeile);
        }
    }
}
