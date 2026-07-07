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

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Erstellt und aktualisiert Buchungssätze für Betriebskostenrechnungen.
    ///
    /// Eine Betriebskostenrechnung erzeugt einen Buchungssatz mit genau einer Zeile:
    ///   Haben Umlage.NkVerrechnungsKonto [Gesamtbetrag]
    ///
    /// Eine Gutschrift (negativer Betrag) dreht Soll und Haben:
    ///   Soll Umlage.NkVerrechnungsKonto [Absolutbetrag]
    ///
    /// Die Soll-Seite wird durch die NK-Anteil-Buchungen (NkAnteileService) ergänzt:
    /// je Vertrag Soll NkBuchungskonto und für Eigenanteile Soll Wohnung.AufwandsKonto.
    /// Die Bezahlung an den Dienstleister ist ein separater Buchungssatz.
    /// </summary>
    public class BetriebskostenrechnungBuchungsService
    {
        private readonly SaverwalterContext _ctx;

        public BetriebskostenrechnungBuchungsService(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Buchungssatz> BucheRechnungAsync(
            Umlage umlage,
            decimal betrag,
            DateOnly datum,
            int betreffendesJahr,
            string? notiz)
        {
            var buchungssatz = new Buchungssatz(datum, $"Betriebskosten {umlage.Typ.Bezeichnung} {betreffendesJahr}")
            {
                Buchungsjahr = betreffendesJahr
            };
            var sollHaben = betrag >= 0 ? SollHaben.Haben : SollHaben.Soll;
            AddZeile(buchungssatz, sollHaben, Math.Abs(betrag), umlage.NkVerrechnungsKonto);
            buchungssatz.Notiz = notiz;
            _ctx.Buchungssaetze.Add(buchungssatz);
            await _ctx.SaveChangesAsync();
            return buchungssatz;
        }

        public async Task AktualisiereBuchungssatzAsync(
            Buchungssatz satz,
            Umlage neueUmlage,
            decimal neuerBetrag,
            DateOnly neuesDatum,
            int neuesJahr,
            string? neueNotiz)
        {
            satz.Buchungsdatum = neuesDatum;
            satz.Buchungsjahr = neuesJahr; // Wirtschaftsjahr folgt dem betreffenden Jahr, nicht dem Datum.
            satz.Beschreibung = $"Betriebskosten {neueUmlage.Typ.Bezeichnung} {neuesJahr}";
            satz.Notiz = neueNotiz;

            var nkZeile = satz.Buchungszeilen
                .FirstOrDefault(z => z.Buchungskonto.BuchungskontoId == neueUmlage.NkVerrechnungsKonto.BuchungskontoId);
            if (nkZeile is not null)
            {
                nkZeile.SollHaben = neuerBetrag >= 0 ? SollHaben.Haben : SollHaben.Soll;
                nkZeile.Betrag = Math.Abs(neuerBetrag);
            }

            await _ctx.SaveChangesAsync();
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
