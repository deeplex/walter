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
using Deeplex.Saverwalter.WebAPI.Services;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Dokumentierter Verzicht auf die Betriebskostenabrechnung eines Vertrags für
    /// ein Jahr — bewusst ohne Buchung. Die Jahresabschlusskontrolle behandelt den
    /// Vertrag dann als erledigt.
    /// </summary>
    [ApiController]
    [Route("api/abrechnungsverzicht")]
    public class AbrechnungsverzichtController : ControllerBase
    {
        public class VerzichtRequest
        {
            public int VertragId { get; set; }
            public int Jahr { get; set; }
            public string Grund { get; set; } = "";
        }

        private readonly SaverwalterContext _ctx;
        private readonly Microsoft.AspNetCore.Authorization.IAuthorizationService _auth;

        public AbrechnungsverzichtController(
            SaverwalterContext ctx,
            Microsoft.AspNetCore.Authorization.IAuthorizationService auth)
        {
            _ctx = ctx;
            _auth = auth;
        }

        /// <summary>Setzt einen Abrechnungsverzicht (oder aktualisiert dessen Grund).</summary>
        [HttpPost]
        public async Task<ActionResult> Setzen([FromBody] VerzichtRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Grund))
                return BadRequest("Für einen Abrechnungsverzicht muss ein Grund angegeben werden.");

            var vertrag = await _ctx.Vertraege
                .Include(v => v.Wohnung)
                .FirstOrDefaultAsync(v => v.VertragId == request.VertragId);
            if (vertrag == null)
                return NotFound("Vertrag nicht gefunden.");

            if (!await CanAccessAllWohnungen(
                    _auth, _ctx, User!, [vertrag.Wohnung.WohnungId], Operations.Update))
                return Forbid();

            var vorhanden = await _ctx.Abrechnungsverzichte
                .FirstOrDefaultAsync(v => v.Vertrag.VertragId == request.VertragId && v.Jahr == request.Jahr);

            if (vorhanden != null)
            {
                vorhanden.Grund = request.Grund;
                vorhanden.Datum = DateOnly.FromDateTime(DateTime.Today);
            }
            else
            {
                _ctx.Abrechnungsverzichte.Add(new Abrechnungsverzicht
                {
                    Vertrag = vertrag,
                    Jahr = request.Jahr,
                    Grund = request.Grund,
                    Datum = DateOnly.FromDateTime(DateTime.Today)
                });
            }

            await _ctx.SaveChangesAsync();
            return Ok();
        }

        /// <summary>Hebt den Abrechnungsverzicht für Vertrag + Jahr wieder auf.</summary>
        [HttpDelete("{vertragId}/{jahr}")]
        public async Task<ActionResult> Aufheben(int vertragId, int jahr)
        {
            var verzicht = await _ctx.Abrechnungsverzichte
                .Include(v => v.Vertrag).ThenInclude(v => v.Wohnung)
                .FirstOrDefaultAsync(v => v.Vertrag.VertragId == vertragId && v.Jahr == jahr);
            if (verzicht == null)
                return NotFound();

            if (!await CanAccessAllWohnungen(
                    _auth, _ctx, User!, [verzicht.Vertrag.Wohnung.WohnungId], Operations.Delete))
                return Forbid();

            _ctx.Abrechnungsverzichte.Remove(verzicht);
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }
}
