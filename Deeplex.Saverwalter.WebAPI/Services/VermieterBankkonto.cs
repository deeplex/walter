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

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public static class VermieterBankkonto
    {
        /// <summary>
        /// Bankkonto des Vermieters für den Zahlungsverkehr eines Vertrags — Vorbelegung
        /// beim Ausgleich einer Abrechnung (Empfänger einer Nachzahlung, Zahler einer
        /// Erstattung). Das Vertrags-Zahlungskonto ist meist ein rein virtuelles
        /// Buchungskonto ohne Bankkonto dahinter; dann greift das Bankkonto der aktuellen
        /// Wohnungs-Eigentümer (gleiche Auflösung wie beim Betriebskosten-Eingang).
        /// </summary>
        public static async Task<int?> ErmittleAsync(SaverwalterContext ctx, Vertrag vertrag)
        {
            var direkt = await ctx.Bankkontos
                .Where(b => b.BuchungsKonto.BuchungskontoId == vertrag.ZahlungsKonto.BuchungskontoId)
                .Select(b => (int?)b.BankkontoId)
                .FirstOrDefaultAsync();
            if (direkt != null) return direkt;

            return await EigentuemerAsync(ctx, vertrag.Wohnung.WohnungId);
        }

        /// <summary>
        /// Bankkonto der aktuellen Eigentümer einer Wohnung — Vorbelegung, wenn der Vermieter
        /// zahlt (z. B. eine Rechnung für Erhaltungsaufwand).
        /// </summary>
        public static async Task<int?> EigentuemerAsync(SaverwalterContext ctx, int wohnungId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var besitzerIds = await ctx.Wohnungen
                .Where(w => w.WohnungId == wohnungId)
                .SelectMany(w => w.Eigentuemer)
                .Where(e => e.Bis == null || e.Bis >= today)
                .Select(e => e.Kontakt.KontaktId)
                .ToListAsync();
            if (besitzerIds.Count == 0) return null;

            return await ctx.Bankkontos
                .Where(b => b.Besitzer.Any(k => besitzerIds.Contains(k.KontaktId)))
                .Select(b => (int?)b.BankkontoId)
                .FirstOrDefaultAsync();
        }
    }
}
