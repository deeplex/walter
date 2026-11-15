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
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Nebenkostenabrechnungen eines Vertrags in der Fachsicht: je Abrechnung die
    /// Nachzahlung bzw. Erstattung, was davon bezahlt ist und was offen ist. Grundlage ist
    /// die Saldo-Zeile auf dem BkAbrechnungsKonto (<see cref="AbrechnungsSaldo"/>); bezahlt
    /// ist, was per Ausgleich daran hängt. Eine stornierte Abrechnung bleibt als Beleg in
    /// der Liste, ist aber nicht mehr offen. Liest nur.
    /// </summary>
    public class NebenkostenabrechnungenService(SaverwalterContext ctx)
    {
        private const decimal Toleranz = 0.005m;

        public enum AbrechnungsStatus { Entwurf, Abgesendet, Storniert }

        public class AbrechnungsZahlung
        {
            public Guid BuchungssatzId { get; set; }
            public Guid? TransaktionId { get; set; }
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public string? Zahler { get; set; }
            public string? Empfaenger { get; set; }
            /// <summary>Erstattung an den Mieter (sonst Nachzahlung des Mieters).</summary>
            public bool Erstattung { get; set; }
        }

        public class Nebenkostenabrechnung
        {
            public Guid AbrechnungsresultatId { get; set; }
            public int Jahr { get; set; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public AbrechnungsStatus Status { get; set; }
            /// <summary>Positiv: Nachzahlung des Mieters; negativ: Erstattung an den Mieter.</summary>
            public decimal Saldo { get; set; }
            /// <summary>Betrag der Nachzahlung bzw. Erstattung.</summary>
            public decimal Betrag => Math.Abs(Saldo);
            public decimal Bezahlt { get; set; }
            public decimal Offen => Status == AbrechnungsStatus.Storniert ? 0m : Betrag - Bezahlt;
            /// <summary>Abgesendet, nicht storniert und noch offen: ein Ausgleich lässt sich buchen.</summary>
            public bool KannAusgleichen => Status == AbrechnungsStatus.Abgesendet && Offen > Toleranz;
            public List<AbrechnungsZahlung> Zahlungen { get; set; } = [];
        }

        public class Nebenkostenabrechnungen
        {
            /// <summary>Neuestes Jahr zuerst.</summary>
            public List<Nebenkostenabrechnung> Abrechnungen { get; set; } = [];
            /// <summary>Stornierte Ausgleichszahlungen; ihr Ausgleich ist mit dem Storno entfallen.</summary>
            public List<AbrechnungsZahlung> RueckgaengigGemacht { get; set; } = [];
            /// <summary>Vorbelegung: Empfänger einer Nachzahlung, Zahler einer Erstattung.</summary>
            public int? VermieterBankkontoId { get; set; }
        }

        public async Task<Nebenkostenabrechnungen> ErmittleAsync(int vertragId)
        {
            var vertrag = await ctx.Vertraege
                .Include(v => v.BkAbrechnungsKonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.Wohnung)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId)
                ?? throw new ArgumentException($"Vertrag {vertragId} nicht gefunden.");

            var resultate = await ctx.Abrechnungsresultate
                .AsSplitQuery()
                .Where(r => r.Vertrag.VertragId == vertragId)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(r => r.Buchungssatz).ThenInclude(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .ToListAsync();

            var abrechnungen = resultate
                .Select(r =>
                {
                    var zahlungen = AbrechnungsSaldo.Zahlungen(r);
                    return new Nebenkostenabrechnung
                    {
                        AbrechnungsresultatId = r.AbrechnungsresultatId,
                        Jahr = r.Buchungssatz.Buchungsjahr,
                        Status = r.Buchungssatz.StornoNach != null ? AbrechnungsStatus.Storniert
                            : r.Abgesendet ? AbrechnungsStatus.Abgesendet
                            : AbrechnungsStatus.Entwurf,
                        Saldo = AbrechnungsSaldo.Saldo(r),
                        Bezahlt = zahlungen.Sum(z => z.Betrag),
                        Zahlungen = zahlungen
                            .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                            .Select(Zahlung)
                            .ToList(),
                    };
                })
                .OrderByDescending(a => a.Jahr)
                .ThenBy(a => a.Status == AbrechnungsStatus.Storniert)
                .ToList();

            var kontoId = vertrag.BkAbrechnungsKonto.BuchungskontoId;
            var rueckgaengig = await ctx.Buchungszeilen
                .Where(z => z.Buchungskonto.BuchungskontoId == kontoId
                    && z.Buchungssatz.Transaktion != null
                    && z.Buchungssatz.StornoNach != null)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                .ToListAsync();

            return new Nebenkostenabrechnungen
            {
                Abrechnungen = abrechnungen,
                RueckgaengigGemacht = rueckgaengig.Select(Zahlung).ToList(),
                VermieterBankkontoId = await VermieterBankkonto.ErmittleAsync(ctx, vertrag),
            };
        }

        private static AbrechnungsZahlung Zahlung(Buchungszeile z)
        {
            var satz = z.Buchungssatz;
            var t = satz.Transaktion;
            return new AbrechnungsZahlung
            {
                BuchungssatzId = satz.BuchungssatzId,
                TransaktionId = t?.TransaktionId,
                Datum = t?.Zahlungsdatum ?? satz.Buchungsdatum,
                Betrag = z.Betrag,
                Zahler = Name(t?.Zahler),
                Empfaenger = Name(t?.Zahlungsempfaenger),
                // Auf dem BkAbrechnungsKonto: Nachzahlung im Haben, Erstattung im Soll.
                Erstattung = z.SollHaben == SollHaben.Soll,
            };
        }

        private static string? Name(Bankkonto? b) => b?.Besitzer.FirstOrDefault()?.Bezeichnung ?? b?.Iban;
    }
}
