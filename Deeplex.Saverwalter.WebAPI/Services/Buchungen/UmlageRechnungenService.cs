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

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Betriebskostenrechnungen einer Umlage in der Fachsicht: je Rechnung Betrag, bezahlt,
    /// offen und wie viel davon auf die Mieter verteilt ist. Eine Rechnung ist die Haben-Zeile
    /// auf dem Verrechnungskonto der Umlage; bezahlt ist, was per Ausgleich daran hängt,
    /// verteilt die gebuchten Soll-Zeilen im selben Satz (<see cref="NkVerteilung"/>).
    /// Storno-Sätze sind keine Rechnungen; eine stornierte Rechnung bleibt als Beleg in der
    /// Liste, ist aber nicht mehr offen. Liest nur.
    /// </summary>
    public class UmlageRechnungenService(SaverwalterContext ctx)
    {
        private const decimal Toleranz = 0.005m;

        public class Rechnung
        {
            public Guid BuchungssatzId { get; set; }
            public int Jahr { get; set; }
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public decimal Bezahlt { get; set; }
            /// <summary>Auf Mieter bzw. Eigenanteile verteilt.</summary>
            public decimal Verteilt { get; set; }
            public bool Storniert { get; set; }
            public decimal Offen => Storniert ? 0m : Betrag - Bezahlt;
            public bool KannBezahlen => !Storniert && Offen > Toleranz;
            public List<RechnungsZahlung> Zahlungen { get; set; } = [];
        }

        public class UmlageRechnungen
        {
            /// <summary>Neuestes Jahr zuerst, im Jahr die neueste Rechnung zuerst.</summary>
            public List<Rechnung> Rechnungen { get; set; } = [];
            /// <summary>Stornierte Zahlungen; ihr Ausgleich ist mit dem Storno entfallen.</summary>
            public List<RechnungsZahlung> RueckgaengigGemacht { get; set; } = [];
        }

        public async Task<UmlageRechnungen> ErmittleAsync(int umlageId)
        {
            var umlage = await ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .FirstOrDefaultAsync(u => u.UmlageId == umlageId)
                ?? throw new ArgumentException($"Umlage {umlageId} nicht gefunden.");
            var kontoId = umlage.NkVerrechnungsKonto.BuchungskontoId;

            var saetze = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Where(s => s.StornoVon == null && s.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == kontoId))
                .Include(s => s.StornoNach)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.AlsSollZeile)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(z => z.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(z => z.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .ToListAsync();

            var rechnungen = saetze
                .Select(s =>
                {
                    var rechnungsZeilen = s.Buchungszeilen
                        .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungskonto.BuchungskontoId == kontoId)
                        .ToList();
                    var zahlungen = rechnungsZeilen.SelectMany(z => z.AlsHabenZeile).Select(a => a.SollZeile).ToList();
                    return new Rechnung
                    {
                        BuchungssatzId = s.BuchungssatzId,
                        Jahr = s.Buchungsjahr,
                        Datum = s.Buchungsdatum,
                        Betrag = rechnungsZeilen.Sum(z => z.Betrag),
                        // Mit einer Abrechnung stornierte Verteil-Zeilen zählen nicht.
                        Verteilt = s.Buchungszeilen
                            .Where(z => z.SollHaben == SollHaben.Soll && NkVerteilung.IstAktiv(z))
                            .Sum(z => z.Betrag),
                        Bezahlt = zahlungen.Sum(z => z.Betrag),
                        Storniert = s.StornoNach != null,
                        Zahlungen = zahlungen
                            .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                            .Select(RechnungsZahlung.Aus)
                            .ToList(),
                    };
                })
                .OrderByDescending(r => r.Jahr)
                .ThenByDescending(r => r.Datum)
                .ToList();

            var rueckgaengig = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == kontoId
                    && z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Transaktion != null
                    && z.Buchungssatz.StornoNach != null)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                .ToListAsync();

            return new UmlageRechnungen
            {
                Rechnungen = rechnungen,
                RueckgaengigGemacht = rueckgaengig.Select(RechnungsZahlung.Aus).ToList(),
            };
        }
    }
}
