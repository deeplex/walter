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
using Deeplex.Saverwalter.WebAPI.Services;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/abrechnungslauf")]
    public class AbrechnungslaufController : ControllerBase
    {
        public class AbrechnungslaufRequest
        {
            public List<int> WohnungIds { get; set; } = [];
        }

        public class AbrechnungslaufGruppenRequest
        {
            public List<AbrechnungslaufRequest> Gruppen { get; set; } = [];
            public int Jahr { get; set; }
        }

        public class RueckabwicklungRequest
        {
            public List<int> WohnungIds { get; set; } = [];
            public int Jahr { get; set; }
            /// <summary>Pflicht beim Storno, beim Löschen ungenutzt.</summary>
            public string? Grund { get; set; }
        }

        private readonly AbrechnungslaufService _service;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;

        public AbrechnungslaufController(
            AbrechnungslaufService service,
            SaverwalterContext ctx,
            IAuthorizationService auth)
        {
            _service = service;
            _ctx = ctx;
            _auth = auth;
        }

        [HttpGet("gruppen")]
        public async Task<ActionResult<List<AbrechnungsGruppe>>> GetGruppen()
        {
            try
            {
                var gruppen = await _service.GetGruppenAsync();

                // Nur Gruppen zurückgeben, deren Wohnungen der Nutzer vollständig
                // lesen darf — sonst würden fremde Abrechnungsgruppen geleakt.
                var sichtbar = new List<AbrechnungsGruppe>();
                foreach (var gruppe in gruppen)
                {
                    if (await CanAccessAllWohnungen(
                            _auth, _ctx, User!, gruppe.WohnungIds, Operations.Read))
                    {
                        sichtbar.Add(gruppe);
                    }
                }

                return sichtbar;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost("preview")]
        public async Task<ActionResult<List<AbrechnungslaufResult>>> Preview([FromBody] AbrechnungslaufGruppenRequest request)
        {
            if (request.Gruppen.Count == 0)
                return BadRequest("Mindestens eine Abrechnungsgruppe muss ausgewählt sein.");

            try
            {
                var results = new List<AbrechnungslaufResult>();

                foreach (var gruppe in request.Gruppen)
                {
                    if (gruppe.WohnungIds.Count == 0)
                        return BadRequest("Abrechnungsgruppe darf nicht leer sein.");

                    if (!await CanAccessAllWohnungen(
                            _auth, _ctx, User!, gruppe.WohnungIds, Operations.Read))
                        return Forbid();

                    var gruppeResult = await _service.PreviewAsync(gruppe.WohnungIds, request.Jahr);
                    results.Add(new AbrechnungslaufResult
                    {
                        Gruppen = [gruppeResult],
                        Warnungen = gruppeResult.Warnungen
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Jahresabschluss-Kontrolle: prüft für ALLE sichtbaren Abrechnungsgruppen des
        /// Jahres, ob eine erneute Abrechnung dasselbe ergäbe wie das Gebuchte. Nur lesend
        /// (Preview je Gruppe), keine Buchung. Kann bei vielen Gruppen dauern → on-demand.
        /// </summary>
        [HttpGet("kontrolle/{jahr:int}")]
        public async Task<ActionResult<JahresabschlussKontrolleResult>> Kontrolle(int jahr)
        {
            try
            {
                var verzichtet = (await _ctx.Abrechnungsverzichte
                    .Where(v => v.Jahr == jahr)
                    .Select(v => v.Vertrag.VertragId)
                    .ToListAsync())
                    .ToHashSet();

                var result = new JahresabschlussKontrolleResult { Jahr = jahr };

                foreach (var gruppe in await _service.GetGruppenAsync())
                {
                    // Nur Gruppen prüfen, die der Nutzer vollständig lesen darf.
                    if (!await CanAccessAllWohnungen(
                            _auth, _ctx, User!, gruppe.WohnungIds, Operations.Read))
                        continue;

                    var preview = await _service.PreviewAsync(gruppe.WohnungIds, jahr);
                    result.Positionen.AddRange(
                        JahresabschlussKontrolle.Klassifiziere(preview, gruppe.Bezeichnung, verzichtet));
                }

                JahresabschlussKontrolle.Aggregiere(result);
                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost("book")]
        public async Task<ActionResult<List<AbrechnungslaufResult>>> Book([FromBody] AbrechnungslaufGruppenRequest request)
        {
            if (request.Gruppen.Count == 0)
                return BadRequest("Mindestens eine Abrechnungsgruppe muss ausgewählt sein.");

            try
            {
                var results = new List<AbrechnungslaufResult>();

                foreach (var gruppe in request.Gruppen)
                {
                    if (gruppe.WohnungIds.Count == 0)
                        return BadRequest("Abrechnungsgruppe darf nicht leer sein.");

                    // Buchen verändert Finanzdaten → Vollmacht (Update) nötig.
                    if (!await CanAccessAllWohnungen(
                            _auth, _ctx, User!, gruppe.WohnungIds, Operations.Update))
                        return Forbid();

                    var gruppeResult = await _service.BookAsync(gruppe.WohnungIds, request.Jahr);
                    results.Add(new AbrechnungslaufResult
                    {
                        Gruppen = [gruppeResult],
                        Warnungen = gruppeResult.Warnungen
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Nimmt die gebuchte Abrechnung der Gruppe für das Jahr vollständig zurück
        /// (Resultate, Verteil-Zeilen, Umbuchungen). Gesperrt bei abgesendeten
        /// Abrechnungen — dann ist nur noch das Gruppen-Storno möglich.
        /// </summary>
        [HttpPost("rueckabwicklung")]
        public async Task<ActionResult<AbrechnungslaufService.RueckabwicklungResult>> Rueckabwicklung(
            [FromBody] RueckabwicklungRequest request)
        {
            if (request.WohnungIds.Count == 0)
                return BadRequest("Abrechnungsgruppe darf nicht leer sein.");

            if (!await CanAccessAllWohnungen(
                    _auth, _ctx, User!, request.WohnungIds, Operations.Delete))
                return Forbid();

            try
            {
                return Ok(await _service.DeleteAsync(request.WohnungIds, request.Jahr));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Storniert die gebuchte Abrechnung der Gruppe GoB-konform — der Weg für
        /// bereits abgesendete Abrechnungen. Grund ist Pflicht.
        /// </summary>
        [HttpPost("storno")]
        public async Task<ActionResult<AbrechnungslaufService.RueckabwicklungResult>> Storno(
            [FromBody] RueckabwicklungRequest request)
        {
            if (request.WohnungIds.Count == 0)
                return BadRequest("Abrechnungsgruppe darf nicht leer sein.");

            if (string.IsNullOrWhiteSpace(request.Grund))
                return BadRequest("Für ein Storno muss ein Grund angegeben werden.");

            if (!await CanAccessAllWohnungen(
                    _auth, _ctx, User!, request.WohnungIds, Operations.Delete))
                return Forbid();

            try
            {
                return Ok(await _service.StornoAsync(request.WohnungIds, request.Jahr, request.Grund));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
