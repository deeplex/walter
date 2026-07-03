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
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Bucht vorberechnete NK-Anteile auf die Buchungssätze der Betriebskostenrechnungen.
    ///
    /// Der Service ist ausschließlich für die Buchung zuständig — die Berechnung
    /// (inkl. Rundungskorrektur) findet im <see cref="NkGruppenAbrechnungsService"/> statt.
    /// Diese Aufteilung verhindert die Doppelberechnung, die der vorherige
    /// <c>NkAnteileService</c> noch über die Legacy-<c>Abrechnungseinheit</c> machte.
    ///
    /// Buchungslogik je Anteil (Soll-Seite, Haben wurde beim Rechnungseingang gebucht):
    ///   Mieter-Partei    → Soll Vertrag.NkBuchungskonto
    ///   Eigenanteil      → Soll Wohnung.AufwandsKonto
    ///
    /// Idempotent: Konten, auf die bereits gebucht wurde, werden übersprungen.
    /// </summary>
    public class NkAnteilBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public NkAnteilBuchungsService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public const string BeschreibungPrefix = NkAnteilMarker;

        /// <summary>
        /// Erstellt einen neuen Buchungssatz für einen manuellen Vertrags-NK-Anteil.
        ///   Soll  Vertrag.NkBuchungskonto     — NK-Forderung gegen den Mieter
        ///   Haben Umlage.NkVerrechnungsKonto  — Umlagekosten verteilt
        /// </summary>
        public async Task<Buchungssatz> BucheVertragsNkAnteilAsync(
            int vertragId, int umlageId, decimal betrag, int betreffendesJahr, DateOnly datum, string? notiz)
        {
            if (betrag <= 0)
                throw new ArgumentException($"Betrag muss größer als 0 sein (aktuell: {betrag:C}).");

            var vertrag = await _ctx.Vertraege
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.Mieter)
                .Include(v => v.Wohnung)
                .FirstOrDefaultAsync(v => v.VertragId == vertragId)
                ?? throw new ArgumentException($"Vertrag {vertragId} nicht gefunden.");

            var umlage = await _ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Typ)
                .FirstOrDefaultAsync(u => u.UmlageId == umlageId)
                ?? throw new ArgumentException($"Umlage {umlageId} nicht gefunden.");

            var beschreibung = $"{BeschreibungPrefix}{umlage.Typ.Bezeichnung} {betreffendesJahr}";
            var satz = new Buchungssatz(datum, beschreibung)
            {
                Buchungsjahr = betreffendesJahr,
                Notiz = notiz
            };
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = vertrag.NkBuchungskonto
            });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = umlage.NkVerrechnungsKonto
            });

            _ctx.Buchungssaetze.Add(satz);
            await _ctx.SaveChangesAsync();
            return satz;
        }

        public sealed class NkAnteilBuchungsResult
        {
            public int GebuchteAnteile { get; init; }
            public int UebersprungeneAnteile { get; init; }
            public List<string> Warnungen { get; init; } = [];
        }

        /// <summary>
        /// Bucht die vorberechneten Anteile einer Rechnung. Die Liste der Anteile kommt
        /// direkt aus dem <see cref="NkRechnungsplan"/> des Abrechnungslaufs.
        /// </summary>
        public async Task<NkAnteilBuchungsResult> BucheAnteileAsync(
            Buchungssatz satz,
            IReadOnlyList<NkRechnungsAnteil> anteile)
        {
            var bereitsGebuchteKontoIds = satz.Buchungszeilen
                .Select(z => z.Buchungskonto.BuchungskontoId)
                .ToHashSet();

            int gebucht = 0, uebersprungen = 0;
            var warnungen = new List<string>();

            foreach (var anteil in anteile)
            {
                var konto = anteil.Partei.Buchungskonto;
                if (bereitsGebuchteKontoIds.Contains(konto.BuchungskontoId))
                {
                    uebersprungen++;
                    continue;
                }

                if (anteil.Betrag == 0)
                {
                    uebersprungen++;
                    continue;
                }

                // Positive Anteile: Soll NkBuchungskonto (Forderung gegen Mieter).
                // Negative Anteile (Gutschrift): Haben NkBuchungskonto (Entlastung des Mieters).
                var sollHaben = anteil.Betrag > 0 ? SollHaben.Soll : SollHaben.Haben;
                var zeile = new Buchungszeile(sollHaben, Math.Abs(anteil.Betrag))
                {
                    Buchungssatz = satz,
                    Buchungskonto = konto
                };
                satz.Buchungszeilen.Add(zeile);
                bereitsGebuchteKontoIds.Add(konto.BuchungskontoId);
                gebucht++;
            }

            await _ctx.SaveChangesAsync();

            return new NkAnteilBuchungsResult
            {
                GebuchteAnteile = gebucht,
                UebersprungeneAnteile = uebersprungen,
                Warnungen = warnungen
            };
        }
    }
}
