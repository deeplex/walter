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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wohnungeigentuemer")]
    public class WohnungEigentuemerController : ControllerBase
    {
        public class WohnungEigentuemerEntry
        {
            public int Id { get; set; }
            public SelectionEntry Wohnung { get; set; } = null!;
            public SelectionEntry Kontakt { get; set; } = null!;
            public DateOnly Von { get; set; }
            public DateOnly? Bis { get; set; }
            public decimal? Anteil { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public WohnungEigentuemerEntry() { }
            public WohnungEigentuemerEntry(WohnungEigentuemer entity)
            {
                Id = entity.WohnungEigentuemerId;
                Wohnung = new(entity.Wohnung.WohnungId, entity.Wohnung.Bezeichnung);
                Kontakt = new(entity.Kontakt.KontaktId, entity.Kontakt.Bezeichnung);
                Von = entity.Von;
                Bis = entity.Bis;
                Anteil = entity.Anteil;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;

        public WohnungEigentuemerController(SaverwalterContext ctx, IAuthorizationService auth)
        {
            _ctx = ctx;
            _auth = auth;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WohnungEigentuemerEntry>>> Get()
        {
            var list = await _ctx.WohnungEigentuemer
                .Include(e => e.Wohnung)
                .Include(e => e.Kontakt)
                .ToListAsync();
            return Ok(list.Select(e => new WohnungEigentuemerEntry(e)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WohnungEigentuemerEntry>> Get(int id)
        {
            var entity = await _ctx.WohnungEigentuemer
                .Include(e => e.Wohnung)
                .Include(e => e.Kontakt)
                .FirstOrDefaultAsync(e => e.WohnungEigentuemerId == id);
            if (entity is null) return NotFound();
            return Ok(new WohnungEigentuemerEntry(entity));
        }

        [HttpPost]
        public async Task<ActionResult<WohnungEigentuemerEntry>> Post([FromBody] WohnungEigentuemerEntry entry)
        {
            if (entry.Id != 0) return BadRequest();

            var wohnung = await _ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
            if (wohnung is null) return BadRequest("Wohnung nicht gefunden.");

            var authRx = await _auth.AuthorizeAsync(User, wohnung, [Operations.SubCreate]);
            if (!authRx.Succeeded) return Forbid();

            var kontakt = await _ctx.Kontakte.FindAsync(entry.Kontakt.Id);
            if (kontakt is null) return BadRequest("Kontakt nicht gefunden.");

            var entity = new WohnungEigentuemer(entry.Von)
            {
                Wohnung = wohnung,
                Kontakt = kontakt,
                Bis = entry.Bis,
                Anteil = entry.Anteil
            };

            _ctx.WohnungEigentuemer.Add(entity);
            await _ctx.SaveChangesAsync();

            return Ok(new WohnungEigentuemerEntry(entity));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WohnungEigentuemerEntry>> Put(int id, [FromBody] WohnungEigentuemerEntry entry)
        {
            var entity = await _ctx.WohnungEigentuemer
                .Include(e => e.Wohnung)
                .Include(e => e.Kontakt)
                .FirstOrDefaultAsync(e => e.WohnungEigentuemerId == id);
            if (entity is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User, entity.Wohnung, [Operations.Update]);
            if (!authRx.Succeeded) return Forbid();

            entity.Von = entry.Von;
            entity.Bis = entry.Bis;
            entity.Anteil = entry.Anteil;

            await _ctx.SaveChangesAsync();
            return Ok(new WohnungEigentuemerEntry(entity));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _ctx.WohnungEigentuemer
                .Include(e => e.Wohnung)
                .FirstOrDefaultAsync(e => e.WohnungEigentuemerId == id);
            if (entity is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User, entity.Wohnung, [Operations.Delete]);
            if (!authRx.Succeeded) return Forbid();

            _ctx.WohnungEigentuemer.Remove(entity);
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }
}
