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
    /// Mietkonto eines Vertrags in der Fachsicht: was war fällig, was kam, was ist offen.
    /// Liest nur, legt keine Sollstellungen an.
    ///
    /// - <b>Kaltmiete und Garagen je Monat.</b> Fällig ist die Sollstellung des Monats (Soll auf
    ///   dem Mietkonto ohne Transaktion, kein Storno, keine Miet-Korrektur); fehlt sie, die
    ///   Miete laut Vertragsversion, solange der Monat im Vertrag liegt und nicht in der Zukunft.
    ///   Bezahlt ist, was per Ausgleich an der Sollstellung hängt — eine Zahlung zählt also
    ///   für den Monat, dem sie zugeordnet wurde, nicht für den Monat ihres Zahlungsdatums.
    /// - <b>NK-Vorauszahlungen je Jahr.</b> Fällig ist die Summe der monatlichen Vorauszahlungen
    ///   laut Vertragsversion; bezahlt ist, was die Abrechnung als Vorauszahlung des
    ///   Buchungsjahres zählt (<see cref="NkVorauszahlung"/>), also einschließlich Miet-Korrekturen.
    /// - <b>Nicht zugeordnet:</b> Zahlungen auf Miet- oder Garagenkonto ohne Ausgleich (Guthaben).
    /// - <b>Rückgängig gemacht:</b> stornierte Zahlungen. Ihr Ausgleich ist mit dem Storno
    ///   entfallen, daher stehen sie nicht mehr bei einem Monat.
    /// </summary>
    public class MietkontoService(SaverwalterContext ctx)
    {
        private const decimal Toleranz = 0.005m;

        public enum MietkontoZahlungsArt { Kaltmiete, Garage, NkVorauszahlung, MietKorrektur }

        public class MietkontoZahlung
        {
            public Guid BuchungssatzId { get; set; }
            public Guid? TransaktionId { get; set; }
            public DateOnly Datum { get; set; }
            /// <summary>Betrag der Zahlung (Miet-Korrektur: Wirkung auf die NK-Vorauszahlung, kann negativ sein).</summary>
            public decimal Betrag { get; set; }
            public string? Zahler { get; set; }
            [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
            public MietkontoZahlungsArt Art { get; set; }
            /// <summary>Kennung der Garage bei <see cref="MietkontoZahlungsArt.Garage"/>.</summary>
            public string? Garage { get; set; }
        }

        public class MietkontoGarage
        {
            public int GarageVertragId { get; set; }
            public string Kennung { get; set; } = string.Empty;
            public decimal Faellig { get; set; }
            public decimal Bezahlt { get; set; }
        }

        public class MietkontoMonat
        {
            public int Jahr { get; set; }
            public int Monat { get; set; }
            public decimal Faellig => Kaltmiete + Garagen.Sum(g => g.Faellig);
            public decimal Bezahlt { get; set; }
            public decimal Offen => Faellig - Bezahlt;
            /// <summary>Fällige Kaltmiete des Monats.</summary>
            public decimal Kaltmiete { get; set; }
            public List<MietkontoGarage> Garagen { get; set; } = [];
            public List<MietkontoZahlung> Zahlungen { get; set; } = [];
        }

        public class MietkontoNkJahr
        {
            public int Jahr { get; set; }
            public decimal Faellig { get; set; }
            public decimal Bezahlt { get; set; }
            public decimal Offen => Faellig - Bezahlt;
            public List<MietkontoZahlung> Zahlungen { get; set; } = [];
        }

        public class Mietkonto
        {
            /// <summary>Neuester Monat zuerst.</summary>
            public List<MietkontoMonat> Monate { get; set; } = [];
            /// <summary>Neuestes Jahr zuerst.</summary>
            public List<MietkontoNkJahr> NkVorauszahlungen { get; set; } = [];
            public List<MietkontoZahlung> NichtZugeordnet { get; set; } = [];
            public List<MietkontoZahlung> RueckgaengigGemacht { get; set; } = [];
            /// <summary>Summe offen; negativ bei Guthaben.</summary>
            public decimal Offen =>
                Monate.Sum(m => m.Offen) + NkVorauszahlungen.Sum(n => n.Offen) - NichtZugeordnet.Sum(z => z.Betrag);
        }

        /// <param name="heute">Stichtag: spätere Monate werden ohne Sollstellung nicht als fällig gezeigt.</param>
        public async Task<Mietkonto> ErmittleAsync(int vertragId, DateOnly heute)
        {
            var vertrag = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.GarageVertraege).ThenInclude(g => g.Versionen)
                .Include(v => v.GarageVertraege).ThenInclude(g => g.Garage)
                .Include(v => v.GarageVertraege).ThenInclude(g => g.MietBuchungskonto)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId)
                ?? throw new ArgumentException($"Vertrag {vertragId} nicht gefunden.");

            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;
            var kontoIds = vertrag.GarageVertraege
                .Select(g => g.MietBuchungskonto.BuchungskontoId)
                .Append(mietKontoId)
                .Append(nkKontoId)
                .ToList();

            var zeilen = await LadeZeilenAsync(kontoIds);

            var aktuellerMonat = ErsterDesMonats(heute);
            var monate = new Dictionary<DateOnly, MietkontoMonat>();

            // Kaltmiete
            var mietZeilen = zeilen.Where(z => z.Buchungskonto.BuchungskontoId == mietKontoId).ToList();
            var kaltmiete = Monatsposten(
                mietZeilen.Where(z => Sollstellung.Ist(z, nkKontoId)),
                VertragsMonate(vertrag.Beginn(), vertrag.Ende, aktuellerMonat),
                m => vertrag.Versionen.Where(v => v.Beginn <= m).MaxBy(v => v.Beginn)?.Grundmiete);
            foreach (var (monat, posten) in kaltmiete)
            {
                var eintrag = MonatEintrag(monate, monat);
                eintrag.Kaltmiete = posten.Faellig;
                eintrag.Bezahlt += posten.Bezahlt;
                eintrag.Zahlungen.AddRange(posten.Zahlungen.Select(z => Zahlung(z, MietkontoZahlungsArt.Kaltmiete)));
            }

            foreach (var gv in vertrag.GarageVertraege)
                TrageGarageEin(monate, gv, zeilen, aktuellerMonat);

            // NK-Vorauszahlungen je Buchungsjahr
            var zaehlt = NkVorauszahlung.Zaehlt(vertrag).Compile();
            var nkZeilen = zeilen.Where(zaehlt).ToList();
            var nkFaellig = VertragsMonate(vertrag.Beginn(), vertrag.Ende, aktuellerMonat)
                .GroupBy(m => m.Year)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(m => vertrag.Versionen.Where(v => v.Beginn <= m).MaxBy(v => v.Beginn)?.Nebenkostenvorauszahlung ?? 0m));
            var nkJahre = nkFaellig.Keys.Union(nkZeilen.Select(z => z.Buchungssatz.Buchungsjahr));
            var nkVorauszahlungen = nkJahre
                .Select(jahr =>
                {
                    var jahresZeilen = nkZeilen.Where(z => z.Buchungssatz.Buchungsjahr == jahr).ToList();
                    return new MietkontoNkJahr
                    {
                        Jahr = jahr,
                        Faellig = nkFaellig.GetValueOrDefault(jahr),
                        Bezahlt = jahresZeilen.Sum(NkVorauszahlung.Betrag),
                        Zahlungen = jahresZeilen
                            .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                            .Select(z =>
                            {
                                var art = z.Buchungssatz.Transaktion != null
                                    ? MietkontoZahlungsArt.NkVorauszahlung
                                    : MietkontoZahlungsArt.MietKorrektur;
                                var zahlung = Zahlung(z, art);
                                zahlung.Betrag = NkVorauszahlung.Betrag(z);
                                return zahlung;
                            })
                            .ToList(),
                    };
                })
                .Where(n => Math.Abs(n.Faellig) > Toleranz || n.Zahlungen.Count > 0)
                .OrderByDescending(n => n.Jahr)
                .ToList();

            var garagen = vertrag.GarageVertraege.ToDictionary(g => g.MietBuchungskonto.BuchungskontoId, g => g.Garage.Kennung);
            MietkontoZahlung ZahlungAufKonto(Buchungszeile z)
            {
                var kontoId = z.Buchungskonto.BuchungskontoId;
                if (kontoId == mietKontoId) return Zahlung(z, MietkontoZahlungsArt.Kaltmiete);
                if (kontoId == nkKontoId) return Zahlung(z, MietkontoZahlungsArt.NkVorauszahlung);
                return GaragenZahlung(z, garagen[kontoId]);
            }

            return new Mietkonto
            {
                Monate = Sortiert(monate),
                NkVorauszahlungen = nkVorauszahlungen,
                NichtZugeordnet = NichtZugeordnet(zeilen.Where(z => z.Buchungskonto.BuchungskontoId != nkKontoId), ZahlungAufKonto),
                RueckgaengigGemacht = RueckgaengigGemacht(zeilen, ZahlungAufKonto),
            };
        }

        /// <summary>
        /// Mietkonto eines Garagenvertrags: die Garagenmiete je Monat, nach denselben Regeln wie
        /// beim Vertrag; ohne NK-Vorauszahlungen. Ist die Garage an einen Wohnungsvertrag gebunden,
        /// erscheinen dieselben Monate auch in dessen Mietkonto.
        /// </summary>
        public async Task<Mietkonto> ErmittleGarageAsync(int garageVertragId, DateOnly heute)
        {
            var gv = await ctx.GarageVertraege
                .Include(g => g.Versionen)
                .Include(g => g.Garage)
                .Include(g => g.MietBuchungskonto)
                .FirstOrDefaultAsync(g => g.GarageVertragId == garageVertragId)
                ?? throw new ArgumentException($"Garagenvertrag {garageVertragId} nicht gefunden.");

            var zeilen = await LadeZeilenAsync([gv.MietBuchungskonto.BuchungskontoId]);
            var monate = new Dictionary<DateOnly, MietkontoMonat>();
            TrageGarageEin(monate, gv, zeilen, ErsterDesMonats(heute));
            MietkontoZahlung ZahlungAufKonto(Buchungszeile z) => GaragenZahlung(z, gv.Garage.Kennung);

            return new Mietkonto
            {
                Monate = Sortiert(monate),
                NichtZugeordnet = NichtZugeordnet(zeilen, ZahlungAufKonto),
                RueckgaengigGemacht = RueckgaengigGemacht(zeilen, ZahlungAufKonto),
            };
        }

        private async Task<List<Buchungszeile>> LadeZeilenAsync(List<int> kontoIds) =>
            await ctx.Buchungszeilen
                .AsSplitQuery()
                .Where(z => kontoIds.Contains(z.Buchungskonto.BuchungskontoId))
                .Include(z => z.Buchungskonto)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Buchungszeilen).ThenInclude(g => g.Buchungskonto)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.StornoVon)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.StornoNach)
                .Include(z => z.Buchungssatz).ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(z => z.AlsSollZeile).ThenInclude(a => a.HabenZeile).ThenInclude(h => h.Buchungssatz)
                    .ThenInclude(s => s.Transaktion).ThenInclude(t => t!.Zahler).ThenInclude(b => b!.Besitzer)
                .Include(z => z.AlsHabenZeile)
                .ToListAsync();

        private static MietkontoMonat MonatEintrag(Dictionary<DateOnly, MietkontoMonat> monate, DateOnly m) =>
            monate.TryGetValue(m, out var e) ? e : monate[m] = new MietkontoMonat { Jahr = m.Year, Monat = m.Month };

        private static List<MietkontoMonat> Sortiert(Dictionary<DateOnly, MietkontoMonat> monate) =>
            monate.Values.OrderByDescending(m => m.Jahr).ThenByDescending(m => m.Monat).ToList();

        /// <summary>Garagenmiete je Monat in die Monate eintragen.</summary>
        private static void TrageGarageEin(
            Dictionary<DateOnly, MietkontoMonat> monate, GarageVertrag gv, List<Buchungszeile> zeilen, DateOnly aktuellerMonat)
        {
            var kontoId = gv.MietBuchungskonto.BuchungskontoId;
            var posten = Monatsposten(
                zeilen.Where(z => z.Buchungskonto.BuchungskontoId == kontoId && Sollstellung.Ist(z)),
                VertragsMonate(gv.Beginn(), gv.Ende, aktuellerMonat),
                m => gv.Versionen.Where(v => v.Beginn <= m).MaxBy(v => v.Beginn)?.GaragenMiete);
            foreach (var (monat, p) in posten)
            {
                var eintrag = MonatEintrag(monate, monat);
                eintrag.Garagen.Add(new MietkontoGarage
                {
                    GarageVertragId = gv.GarageVertragId,
                    Kennung = gv.Garage.Kennung,
                    Faellig = p.Faellig,
                    Bezahlt = p.Bezahlt,
                });
                eintrag.Bezahlt += p.Bezahlt;
                eintrag.Zahlungen.AddRange(p.Zahlungen.Select(z => GaragenZahlung(z, gv.Garage.Kennung)));
            }
        }

        /// <summary>Zahlungen ohne Ausgleich (Guthaben).</summary>
        private static List<MietkontoZahlung> NichtZugeordnet(
            IEnumerable<Buchungszeile> zeilen, Func<Buchungszeile, MietkontoZahlung> zahlung) =>
            zeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Transaktion != null
                    && !IstStornoBeteiligt(z.Buchungssatz)
                    && z.AlsHabenZeile.Count == 0)
                .OrderBy(z => z.Buchungssatz.Buchungsdatum)
                .Select(zahlung)
                .ToList();

        /// <summary>Stornierte Zahlungen: je Satz eine Zeile mit der Summe auf den geladenen Konten.</summary>
        private static List<MietkontoZahlung> RueckgaengigGemacht(
            IEnumerable<Buchungszeile> zeilen, Func<Buchungszeile, MietkontoZahlung> zahlung) =>
            zeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Transaktion != null
                    && z.Buchungssatz.StornoNach != null)
                .GroupBy(z => z.Buchungssatz)
                .OrderBy(g => g.Key.Buchungsdatum)
                .Select(g =>
                {
                    var eintrag = zahlung(g.First());
                    eintrag.Betrag = g.Sum(z => z.Betrag);
                    return eintrag;
                })
                .ToList();

        private static MietkontoZahlung GaragenZahlung(Buchungszeile z, string kennung)
        {
            var zahlung = Zahlung(z, MietkontoZahlungsArt.Garage);
            zahlung.Garage = kennung;
            return zahlung;
        }

        private record Posten(decimal Faellig, decimal Bezahlt, List<Buchungszeile> Zahlungen);

        /// <summary>
        /// Je Monat: Sollstellungen samt Ausgleichen; Vertragsmonate ohne Sollstellung mit der
        /// erwarteten Miete und nichts bezahlt.
        /// </summary>
        private static Dictionary<DateOnly, Posten> Monatsposten(
            IEnumerable<Buchungszeile> sollstellungen,
            IEnumerable<DateOnly> vertragsMonate,
            Func<DateOnly, decimal?> erwartet)
        {
            var result = sollstellungen
                .GroupBy(z => ErsterDesMonats(z.Buchungssatz.Buchungsdatum))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var zahlungen = g.SelectMany(z => z.AlsSollZeile).Select(a => a.HabenZeile).ToList();
                        return new Posten(g.Sum(z => z.Betrag), zahlungen.Sum(h => h.Betrag), zahlungen);
                    });

            foreach (var monat in vertragsMonate)
            {
                if (result.ContainsKey(monat)) continue;
                if (erwartet(monat) is decimal betrag && betrag > Toleranz)
                    result[monat] = new Posten(betrag, 0m, []);
            }
            return result;
        }

        /// <summary>Monate von Vertragsbeginn bis Vertragsende, höchstens bis zum aktuellen Monat.</summary>
        private static IEnumerable<DateOnly> VertragsMonate(DateOnly beginn, DateOnly? ende, DateOnly aktuellerMonat)
        {
            if (beginn == default) yield break;
            var bis = ende is DateOnly e && ErsterDesMonats(e) < aktuellerMonat ? ErsterDesMonats(e) : aktuellerMonat;
            for (var m = ErsterDesMonats(beginn); m <= bis; m = m.AddMonths(1))
                yield return m;
        }

        private static bool IstStornoBeteiligt(Buchungssatz s) => s.StornoVon != null || s.StornoNach != null;

        private static DateOnly ErsterDesMonats(DateOnly d) => new(d.Year, d.Month, 1);

        private static MietkontoZahlung Zahlung(Buchungszeile z, MietkontoZahlungsArt art)
        {
            var satz = z.Buchungssatz;
            var zahler = satz.Transaktion?.Zahler;
            return new MietkontoZahlung
            {
                BuchungssatzId = satz.BuchungssatzId,
                TransaktionId = satz.Transaktion?.TransaktionId,
                Datum = satz.Transaktion?.Zahlungsdatum ?? satz.Buchungsdatum,
                Betrag = z.Betrag,
                Zahler = zahler?.Besitzer.FirstOrDefault()?.Bezeichnung ?? zahler?.Iban,
                Art = art,
            };
        }
    }
}
