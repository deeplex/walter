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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;
using Operations = Deeplex.Saverwalter.WebAPI.Services.Operations;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/buchungssaetze")]
    public class BuchungssaetzeController(
        StornoBuchungsService stornoService,
        SaverwalterContext ctx,
        IAuthorizationService auth) : ControllerBase
    {
        public class StornoBuchungssatzInfo
        {
            public Guid BuchungssatzId { get; set; }
            public int Buchungsnummer { get; set; }
            public int Buchungsjahr { get; set; }
            public DateOnly Buchungsdatum { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
        }

        /// <summary>
        /// Erstellt eine Stornobuchung für den angegebenen Buchungssatz.
        /// Alle Buchungszeilen werden mit umgekehrten Soll/Haben-Seiten gebucht,
        /// bestehende OPOS-Ausgleiche der Originalzeilen werden gelöscht.
        /// </summary>
        [HttpPost("{id}/storno")]
        public async Task<ActionResult<StornoBuchungssatzInfo>> Stornieren(Guid id)
        {
            // Autorisierung: Admin oder Vollzugriff auf die Wohnung des ersten Buchungskontos
            var satz = await ctx.Buchungssaetze.FindAsync(id);
            if (satz == null) return NotFound();

            if (!User!.IsInRole("Admin"))
            {
                // Storno erfordert Vollzugriff — prüfe über die Wohnung des ersten Buchungskontos
                var wohnung = await ctx.Buchungssaetze
                    .Where(s => s.BuchungssatzId == id)
                    .SelectMany(s => s.Buchungszeilen)
                    .Select(z => z.Buchungskonto)
                    .SelectMany(k =>
                        ctx.Vertraege
                            .Where(v =>
                                v.MietBuchungskonto.BuchungskontoId == k.BuchungskontoId ||
                                v.NkBuchungskonto.BuchungskontoId == k.BuchungskontoId ||
                                v.KautionsKonto.BuchungskontoId == k.BuchungskontoId ||
                                v.ZahlungsKonto.BuchungskontoId == k.BuchungskontoId)
                            .Select(v => v.Wohnung))
                    .FirstOrDefaultAsync();

                if (wohnung == null) return Forbid();

                var authRx = await auth.AuthorizeAsync(User!, wohnung, [Operations.Delete]);
                if (!authRx.Succeeded) return Forbid();
            }

            try
            {
                var storno = await stornoService.StornierenAsync(id);
                return Ok(new StornoBuchungssatzInfo
                {
                    BuchungssatzId = storno.BuchungssatzId,
                    Buchungsnummer = storno.Buchungsnummer,
                    Buchungsjahr = storno.Buchungsjahr,
                    Buchungsdatum = storno.Buchungsdatum,
                    Beschreibung = storno.Beschreibung,
                });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }
    }
}
