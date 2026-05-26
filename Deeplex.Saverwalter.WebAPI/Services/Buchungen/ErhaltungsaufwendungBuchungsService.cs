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
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Erstellt Buchungssätze für Erhaltungsaufwendungen (§6 EStG).
    ///
    /// Buchungsschema (doppelte Buchführung):
    ///   Soll  Wohnung.AufwandsKonto       [Betrag]  — Aufwand steigt
    ///   Haben Aussteller.VerbindlichkeitsKonto [Betrag]  — Verbindlichkeit gegenüber Handwerker
    ///
    /// Wird kein VerbindlichkeitsKonto für den Aussteller gefunden, wird es automatisch angelegt.
    /// </summary>
    public class ErhaltungsaufwendungBuchungsService(SaverwalterContext ctx)
    {
        public async Task<Buchungssatz> BucheErhaltungsaufwendungAsync(
            Wohnung wohnung,
            Kontakt aussteller,
            decimal betrag,
            DateOnly datum,
            string bezeichnung,
            string? notiz)
        {
            if (betrag <= 0)
                throw new InvalidOperationException($"Betrag muss größer als 0 sein (aktueller Wert: {betrag:C}).");

            var verbindlichkeitsKonto = await EnsureVerbindlichkeitsKontoAsync(aussteller);

            var buchungssatz = new Buchungssatz(datum, $"Erhaltungsaufwendung: {bezeichnung}");
            buchungssatz.Notiz = notiz;
            AddZeile(buchungssatz, SollHaben.Soll, betrag, wohnung.AufwandsKonto);
            AddZeile(buchungssatz, SollHaben.Haben, betrag, verbindlichkeitsKonto);
            ctx.Buchungssaetze.Add(buchungssatz);
            await ctx.SaveChangesAsync();
            return buchungssatz;
        }

        private async Task<Buchungskonto> EnsureVerbindlichkeitsKontoAsync(Kontakt aussteller)
        {
            if (aussteller.VerbindlichkeitsKonto is not null)
                return aussteller.VerbindlichkeitsKonto;

            var konto = new Buchungskonto(
                $"VK-{aussteller.KontaktId}",
                $"Verbindlichkeiten {aussteller.Bezeichnung}",
                BuchungskontoTyp.Passiv);

            ctx.Buchungskonten.Add(konto);
            aussteller.VerbindlichkeitsKonto = konto;
            await ctx.SaveChangesAsync();
            return konto;
        }

        private static void AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            satz.Buchungszeilen.Add(new Buchungszeile(sollHaben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = konto
            });
        }
    }
}
