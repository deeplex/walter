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
    public class StornoBuchungsService(SaverwalterContext ctx)
    {
        /// <summary>
        /// Erstellt eine Stornobuchung für den angegebenen Buchungssatz.
        /// Alle Buchungszeilen werden mit umgekehrten Soll/Haben-Seiten gebucht.
        /// Bestehende OPOS-Ausgleiche der Originalzeilen werden entfernt.
        /// Der Grund wird als Notiz am Storno-Satz festgehalten.
        /// </summary>
        public async Task<Buchungssatz> StornierenAsync(Guid buchungssatzId, string? grund = null)
        {
            var original = await ctx.Buchungssaetze
                .Include(s => s.StornoNach)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsSollZeile)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.AlsHabenZeile)
                .Include(s => s.Buchungszeilen)
                    .ThenInclude(z => z.Buchungskonto)
                .FirstOrDefaultAsync(s => s.BuchungssatzId == buchungssatzId);

            if (original == null)
                throw new KeyNotFoundException($"Buchungssatz {buchungssatzId} nicht gefunden.");

            if (original.StornoNach != null)
                throw new InvalidOperationException(
                    $"Buchungssatz {buchungssatzId} wurde bereits storniert (Storno-Buchungssatz: {original.StornoNach.BuchungssatzId}).");

            var storno = new Buchungssatz(
                DateOnly.FromDateTime(DateTime.Today),
                $"Storno: {original.Beschreibung}")
            {
                Buchungsjahr = original.Buchungsjahr,
                StornoVon = original,
                Notiz = string.IsNullOrWhiteSpace(grund) ? null : grund.Trim(),
            };

            foreach (var zeile in original.Buchungszeilen)
            {
                // OPOS-Ausgleiche der Originalzeile entfernen
                ctx.OffenePostenAusgleiche.RemoveRange(zeile.AlsSollZeile);
                ctx.OffenePostenAusgleiche.RemoveRange(zeile.AlsHabenZeile);

                // Buchungszeile mit umgekehrter Seite
                var stornoZeile = new Buchungszeile(
                    zeile.SollHaben == SollHaben.Soll ? SollHaben.Haben : SollHaben.Soll,
                    zeile.Betrag)
                {
                    Buchungssatz = storno,
                    Buchungskonto = zeile.Buchungskonto,
                };
                storno.Buchungszeilen.Add(stornoZeile);
            }

            ctx.Buchungssaetze.Add(storno);
            await ctx.SaveChangesAsync();

            return storno;
        }
    }
}
