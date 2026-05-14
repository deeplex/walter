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
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.AspNetCore.Mvc;

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

        private readonly AbrechnungslaufService _service;

        public AbrechnungslaufController(AbrechnungslaufService service)
        {
            _service = service;
        }

        [HttpGet("gruppen")]
        public async Task<ActionResult<List<AbrechnungsGruppe>>> GetGruppen()
        {
            try
            {
                return await _service.GetGruppenAsync();
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
    }
}
