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
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Services.JahresabschlussService;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    /// <summary>
    /// Jahresabschlusskontrolle: Balance-Sheet pro Abrechnungszeitraum. Rein lesend.
    /// </summary>
    [ApiController]
    [Route("api/jahresabschluss")]
    public class JahresabschlussController(SaverwalterContext ctx) : ControllerBase
    {
        /// <summary>
        /// Sichtbare Konto-IDs des Nutzers — null bedeutet Admin (alle).
        /// </summary>
        private async Task<HashSet<int>?> ScopedKontoIds()
        {
            if (User.IsInRole("Admin"))
            {
                return null;
            }

            var ids = await TransaktionPermissionHandler
                .ManagedBuchungskontoIds(ctx, User.GetUserId(), VerwalterRolle.Keine)
                .ToListAsync();
            return ids.ToHashSet();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JahresUebersichtEntry>>> GetUebersicht()
        {
            var scopedKontoIds = await ScopedKontoIds();
            return Ok(await UebersichtAsync(ctx, scopedKontoIds));
        }

        [HttpGet("{jahr}")]
        public async Task<ActionResult<JahresabschlussEntry>> Get(int jahr)
        {
            var scopedKontoIds = await ScopedKontoIds();
            return Ok(await ForJahrAsync(ctx, jahr, scopedKontoIds));
        }
    }
}
