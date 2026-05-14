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
    /// Erstellt Buchungssätze für Mietzahlungen und zugehörige Sollstellungen.
    ///
    /// Eine Mietzahlung erzeugt bis zu zwei Buchungssätze:
    ///   Kaltmiete: Soll Vertrag.ZahlungsKonto / Haben Vertrag.MietBuchungskonto
    ///   NK-VZ:     Soll Vertrag.ZahlungsKonto / Haben Vertrag.NkBuchungskonto
    ///
    /// Fehlt die Sollstellung für den Monat, wird sie automatisch angelegt:
    ///   Soll Vertrag.MietBuchungskonto / Haben Wohnung.MietErtragskonto
    ///   Buchungsdatum = 3. Werktag des Monats
    /// </summary>
    public class MietzahlungBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public MietzahlungBuchungsService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public record MietzahlungInput(
            Vertrag Vertrag,
            DateOnly BetreffenderMonat,
            DateOnly Zahlungsdatum,
            decimal KaltmieteZahlung,
            decimal NkZahlung,
            string? Notiz);

        public record MietzahlungErgebnis(
            Buchungssatz SollstellungBuchungssatz,
            decimal SollstellungBetrag,
            decimal VerbleibendeForderung,
            Buchungssatz KaltmieteBuchungssatz,
            Buchungssatz? NkBuchungssatz);

        public async Task<MietzahlungErgebnis> BucheMietzahlungAsync(MietzahlungInput input)
        {
            var monat = new DateOnly(input.BetreffenderMonat.Year, input.BetreffenderMonat.Month, 1);

            var version = input.Vertrag.Versionen
                .Where(v => v.Beginn <= monat)
                .MaxBy(v => v.Beginn)
                ?? throw new InvalidOperationException($"Keine VertragVersion für {monat:MM/yyyy} gefunden.");

            if (input.KaltmieteZahlung < 0)
                throw new InvalidOperationException($"Kaltmiete-Zahlung darf nicht negativ sein (aktueller Wert: {input.KaltmieteZahlung:C}).");

            if (input.NkZahlung < 0)
                throw new InvalidOperationException($"NK-VZ-Zahlung darf nicht negativ sein (aktueller Wert: {input.NkZahlung:C}).");

            if (input.KaltmieteZahlung > version.Grundmiete)
                throw new InvalidOperationException($"Kaltmiete-Zahlung ({input.KaltmieteZahlung:C}) darf nicht höher als Grundmiete ({version.Grundmiete:C}) sein.");

            var sollstellung = await FindeOderErstelleSollstellungAsync(input.Vertrag, monat);

            var forderungBetrag = sollstellung.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll
                    && z.Buchungskonto.BuchungskontoId == input.Vertrag.MietBuchungskonto.BuchungskontoId)
                .Sum(z => z.Betrag);

            var schonGezahlt = input.Vertrag.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                    && z.Buchungssatz.Buchungsdatum.Month == monat.Month)
                .Sum(z => z.Betrag);

            var verbleibend = forderungBetrag - schonGezahlt;

            if (input.KaltmieteZahlung > verbleibend)
                throw new InvalidOperationException(
                    $"Kaltmiete-Zahlung ({input.KaltmieteZahlung:C}) übersteigt verbleibende Forderung ({verbleibend:C}).");

            var kaltmieteSatz = new Buchungssatz(input.Zahlungsdatum, $"Mietzahlung Kaltmiete {monat:MM/yyyy}");
            AddZeile(kaltmieteSatz, SollHaben.Soll, input.KaltmieteZahlung, input.Vertrag.ZahlungsKonto);
            AddZeile(kaltmieteSatz, SollHaben.Haben, input.KaltmieteZahlung, input.Vertrag.MietBuchungskonto);
            kaltmieteSatz.Notiz = input.Notiz;
            _ctx.Buchungssaetze.Add(kaltmieteSatz);

            Buchungssatz? nkSatz = null;
            if (input.NkZahlung > 0)
            {
                nkSatz = new Buchungssatz(input.Zahlungsdatum, $"Mietzahlung NK-VZ {monat:MM/yyyy}");
                AddZeile(nkSatz, SollHaben.Soll, input.NkZahlung, input.Vertrag.ZahlungsKonto);
                AddZeile(nkSatz, SollHaben.Haben, input.NkZahlung, input.Vertrag.NkBuchungskonto);
                nkSatz.Notiz = input.Notiz;
                _ctx.Buchungssaetze.Add(nkSatz);
            }

            await _ctx.SaveChangesAsync();

            return new MietzahlungErgebnis(sollstellung, forderungBetrag, verbleibend - input.KaltmieteZahlung, kaltmieteSatz, nkSatz);
        }

        /// <summary>
        /// Gibt den Saldo der Mietforderung für einen Monat zurück.
        /// Positiv = noch offen, Null/Negativ = überdeckt.
        /// </summary>
        public decimal BerechneVerbleibendeForderung(Vertrag vertrag, DateOnly monat)
        {
            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;

            var sollSumme = vertrag.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                    && z.Buchungssatz.Buchungsdatum.Month == monat.Month)
                .Sum(z => z.Betrag);

            var habenSumme = vertrag.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                    && z.Buchungssatz.Buchungsdatum.Month == monat.Month)
                .Sum(z => z.Betrag);

            return sollSumme - habenSumme;
        }

        /// <summary>
        /// Gibt alle Kaltmiete-Zahlungs-Buchungssätze für einen Vertrag zurück.
        /// Identifiziert durch: Haben-Buchungszeile auf Vertrag.MietBuchungskonto.
        /// </summary>
        public async Task<List<Buchungssatz>> GetKaltmieteZahlungenAsync(Vertrag vertrag)
        {
            var kontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            return await _ctx.Buchungssaetze
                .Where(s => s.Buchungszeilen.Any(z =>
                    z.SollHaben == SollHaben.Haben
                    && z.Buchungskonto.BuchungskontoId == kontoId))
                .ToListAsync();
        }

        private async Task<Buchungssatz> FindeOderErstelleSollstellungAsync(Vertrag vertrag, DateOnly monat)
        {
            var kontoId = vertrag.MietBuchungskonto.BuchungskontoId;

            var existing = await _ctx.Buchungssaetze
                .FirstOrDefaultAsync(s =>
                    s.Buchungszeilen.Any(z =>
                        z.SollHaben == SollHaben.Soll
                        && z.Buchungskonto.BuchungskontoId == kontoId
                        && z.Buchungssatz.Buchungsdatum.Year == monat.Year
                        && z.Buchungssatz.Buchungsdatum.Month == monat.Month));

            if (existing != null) return existing;

            var version = vertrag.Versionen
                .Where(v => v.Beginn <= monat)
                .MaxBy(v => v.Beginn)
                ?? throw new InvalidOperationException($"Keine VertragVersion für {monat:MM/yyyy} gefunden.");

            var fälligkeitsdatum = DritterWerktag(monat);
            var satz = new Buchungssatz(fälligkeitsdatum, $"Mietsoll {monat:MM/yyyy}");
            AddZeile(satz, SollHaben.Soll, version.Grundmiete, vertrag.MietBuchungskonto);
            AddZeile(satz, SollHaben.Haben, version.Grundmiete, vertrag.Wohnung.MietErtragskonto);
            _ctx.Buchungssaetze.Add(satz);

            return satz;
        }

        /// <summary>
        /// §556b Abs. 1 BGB: Miete ist spätestens am 3. Werktag des Zeitraums fällig.
        /// Werktag = Mo–Sa ohne gesetzliche Feiertage. Feiertage werden hier nicht berücksichtigt,
        /// da sie bundeslandabhängig sind und das Buchungsdatum kein Rechtsdokument ist.
        /// TODO: Feiertagskalender nach Bundesland (Wohnung.Adresse) einbeziehen.
        /// </summary>
        private static DateOnly DritterWerktag(DateOnly monat)
        {
            var tag = new DateOnly(monat.Year, monat.Month, 1);
            int werktage = 0;
            while (werktage < 3)
            {
                if (tag.DayOfWeek != DayOfWeek.Sunday)
                    werktage++;
                if (werktage < 3)
                    tag = tag.AddDays(1);
            }
            return tag;
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
