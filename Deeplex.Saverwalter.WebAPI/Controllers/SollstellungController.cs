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

using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Stößt das Anlegen der fehlenden Sollstellungen an. Im Normalfall erledigt das der
    /// <see cref="SollstellungHintergrunddienst"/>; dieser Endpunkt dient dem Nachholen, etwa für
    /// ein zurückliegendes Jahr.
    /// </summary>
    [ApiController]
    [Route("api/sollstellungen")]
    [Authorize(Policy = "RequireAdmin")]
    public class SollstellungController(SollstellungService service) : ControllerBase
    {
        public class SollstellungRequest
        {
            /// <summary>Das Jahr; ohne Angabe das laufende.</summary>
            public int? Jahr { get; set; }
        }

        public class SollstellungResult
        {
            public int Jahr { get; set; }
            public int Angelegt { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<SollstellungResult>> Post([FromBody] SollstellungRequest? request)
        {
            var heute = DateOnly.FromDateTime(DateTime.Today);
            var jahr = request?.Jahr ?? heute.Year;
            if (jahr < 1900 || jahr > heute.Year)
                return BadRequest($"Jahr {jahr} ist nicht möglich: Sollstellungen gibt es höchstens bis zum laufenden Jahr.");

            var angelegt = await service.AnlegenAsync(jahr, heute);
            return Ok(new SollstellungResult { Jahr = jahr, Angelegt = angelegt });
        }
    }
}
