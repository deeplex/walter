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

using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Erhaltungsaufwand einer Wohnung in der Fachsicht: die Rechnungen mit bezahlt und offen
    /// sowie ein Jahresüberblick. Liest nur.
    ///
    /// Auf dem AufwandsKonto der Wohnung landen zwei Arten von Aufwand, unterschieden an den
    /// Konten des Satzes: Sätze mit einer Zeile auf einem Verrechnungskonto einer Umlage sind
    /// Betriebskosten-Eigenanteile (Verteilung einer Rechnung, z. B. bei Leerstand); alle
    /// anderen sind Erhaltungsaufwand.
    ///
    /// Eine Rechnung für Erhaltungsaufwand steht entweder gegen das Verbindlichkeitskonto
    /// des Ausstellers (erfasst, bezahlt ist, was per Ausgleich daran hängt) oder direkt
    /// gegen ein Bankkonto (gleich bezahlt; die Zahlung ist der Satz selbst). Eine stornierte
    /// Rechnung bleibt als Beleg in der Liste, ist aber nicht mehr offen.
    /// </summary>
    public class ErhaltungsaufwandService(SaverwalterContext ctx)
    {
        private const decimal Toleranz = 0.005m;

        public class Rechnung
        {
            public Guid BuchungssatzId { get; set; }
            public DateOnly Datum { get; set; }
            public int Jahr { get; set; }
            /// <summary>Bezeichnung der Rechnung (nur zur Anzeige).</summary>
            public string Bezeichnung { get; set; } = "";
            /// <summary>Aussteller (Verbindlichkeitskonto), bei direkter Zahlung der Empfänger.</summary>
            public string? Aussteller { get; set; }
            public decimal Betrag { get; set; }
            public decimal Bezahlt { get; set; }
            public bool Storniert { get; set; }
            /// <summary>Gleich bei der Erfassung bezahlt (gegen ein Bankkonto gebucht).</summary>
            public bool DirektBezahlt { get; set; }
            public decimal Offen => Storniert ? 0m : Betrag - Bezahlt;
            /// <summary>Erfasst, nicht storniert und noch offen: eine Zahlung lässt sich buchen.</summary>
            public bool KannBezahlen => !Storniert && !DirektBezahlt && Offen > Toleranz;
            public List<RechnungsZahlung> Zahlungen { get; set; } = [];
        }

        public class Jahr
        {
            public int Wert { get; set; }
            /// <summary>Kaltmiete laut Sollstellung (Saldo des Mietertragskontos).</summary>
            public decimal Mieteinnahmen { get; set; }
            public decimal Erhaltungsaufwand { get; set; }
            /// <summary>Betriebskosten, die nicht auf Mieter verteilt wurden (z. B. Leerstand).</summary>
            public decimal BetriebskostenEigenanteil { get; set; }
        }

        public class Erhaltungsaufwand
        {
            /// <summary>Neueste zuerst.</summary>
            public List<Rechnung> Rechnungen { get; set; } = [];
            /// <summary>Neuestes Jahr zuerst.</summary>
            public List<Jahr> Jahre { get; set; } = [];
            /// <summary>Vorbelegung: Bankkonto der Eigentümer als Zahler.</summary>
            public int? VermieterBankkontoId { get; set; }
        }

        public async Task<Erhaltungsaufwand> ErmittleAsync(int wohnungId)
        {
            var wohnung = await ctx.Wohnungen
                .Include(w => w.AufwandsKonto)
                .Include(w => w.MietErtragskonto)
                .FirstOrDefaultAsync(w => w.WohnungId == wohnungId)
                ?? throw new ArgumentException($"Wohnung {wohnungId} nicht gefunden.");
            var aufwandId = wohnung.AufwandsKonto.BuchungskontoId;

            var umlageKontoIds = (await ctx.Umlagen
                    .Select(u => new { Nk = u.NkVerrechnungsKonto.BuchungskontoId, Sonder = (int?)u.NkSonderVerrechnungsKonto!.BuchungskontoId })
                    .ToListAsync())
                .SelectMany(u => u.Sonder is int s ? new[] { u.Nk, s } : [u.Nk])
                .ToHashSet();

            var saetze = await ctx.Buchungssaetze
                .AsSplitQuery()
                .Where(s => s.Buchungszeilen.Any(z => z.Buchungskonto.BuchungskontoId == aufwandId))
                .Include(s => s.StornoVon)
                .Include(s => s.StornoNach)
                .Include(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(z => z.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile).ThenInclude(a => a.SollZeile).ThenInclude(z => z.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahlungsempfaenger).ThenInclude(b => b!.Besitzer)
                .ToListAsync();

            bool IstBetriebskosten(Buchungssatz s) =>
                s.Buchungszeilen.Any(z => umlageKontoIds.Contains(z.Buchungskonto.BuchungskontoId));

            var gegenKontoIds = saetze
                .SelectMany(s => s.Buchungszeilen)
                .Where(z => z.SollHaben == SollHaben.Haben)
                .Select(z => z.Buchungskonto.BuchungskontoId)
                .ToHashSet();
            var aussteller = (await ctx.Kontakte
                    .Where(k => k.VerbindlichkeitsKonto != null
                        && gegenKontoIds.Contains(k.VerbindlichkeitsKonto.BuchungskontoId))
                    .Include(k => k.VerbindlichkeitsKonto)
                    .ToListAsync())
                .ToDictionary(k => k.VerbindlichkeitsKonto!.BuchungskontoId);

            var rechnungen = saetze
                .Where(s => s.StornoVon == null && !IstBetriebskosten(s)
                    && s.Buchungszeilen.Any(z => z.SollHaben == SollHaben.Soll && z.Buchungskonto.BuchungskontoId == aufwandId))
                .Select(s => ZuRechnung(s, aufwandId, aussteller))
                .OrderByDescending(r => r.Datum)
                .ToList();

            decimal Aufwand(IEnumerable<Buchungssatz> liste) => liste
                .SelectMany(s => s.Buchungszeilen)
                .Where(z => z.Buchungskonto.BuchungskontoId == aufwandId)
                .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);

            var ertragId = wohnung.MietErtragskonto.BuchungskontoId;
            var mieten = (await ctx.Buchungszeilen
                    .Where(z => z.Buchungskonto.BuchungskontoId == ertragId)
                    .Select(z => new { z.Buchungssatz.Buchungsjahr, z.SollHaben, z.Betrag })
                    .ToListAsync())
                .GroupBy(z => z.Buchungsjahr)
                .ToDictionary(g => g.Key, g => g.Sum(z => z.SollHaben == SollHaben.Haben ? z.Betrag : -z.Betrag));

            var jahre = saetze.Select(s => s.Buchungsjahr)
                .Concat(mieten.Keys)
                .Distinct()
                .OrderByDescending(j => j)
                .Select(j =>
                {
                    var imJahr = saetze.Where(s => s.Buchungsjahr == j).ToList();
                    return new Jahr
                    {
                        Wert = j,
                        Mieteinnahmen = mieten.GetValueOrDefault(j),
                        Erhaltungsaufwand = Aufwand(imJahr.Where(s => !IstBetriebskosten(s))),
                        BetriebskostenEigenanteil = Aufwand(imJahr.Where(IstBetriebskosten)),
                    };
                })
                .ToList();

            return new Erhaltungsaufwand
            {
                Rechnungen = rechnungen,
                Jahre = jahre,
                VermieterBankkontoId = await VermieterBankkonto.EigentuemerAsync(ctx, wohnungId),
            };
        }

        private static Rechnung ZuRechnung(Buchungssatz s, int aufwandId, Dictionary<int, Kontakt> aussteller)
        {
            var betrag = s.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungskonto.BuchungskontoId == aufwandId)
                .Sum(z => z.Betrag);
            var gegenZeile = s.Buchungszeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Haben);
            var rechnung = new Rechnung
            {
                BuchungssatzId = s.BuchungssatzId,
                Datum = s.Buchungsdatum,
                Jahr = s.Buchungsjahr,
                Bezeichnung = s.Beschreibung,
                Betrag = betrag,
                Storniert = s.StornoNach != null,
            };

            if (gegenZeile != null && aussteller.TryGetValue(gegenZeile.Buchungskonto.BuchungskontoId, out var kontakt))
            {
                var zahlungen = gegenZeile.AlsHabenZeile.Select(a => a.SollZeile).ToList();
                rechnung.Aussteller = kontakt.Bezeichnung;
                rechnung.Bezahlt = zahlungen.Sum(z => z.Betrag);
                rechnung.Zahlungen = zahlungen
                    .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                    .Select(RechnungsZahlung.Aus)
                    .ToList();
            }
            else if (gegenZeile != null)
            {
                // Direkt bezahlt: der Satz ist zugleich die Zahlung.
                var zahlung = RechnungsZahlung.Aus(gegenZeile);
                rechnung.DirektBezahlt = true;
                rechnung.Aussteller = zahlung.Empfaenger;
                rechnung.Bezahlt = betrag;
                rechnung.Zahlungen = [zahlung];
            }

            return rechnung;
        }
    }
}
