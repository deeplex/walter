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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontaktmitgliedschaften")]
    public class KontaktMitgliedschaftController : ControllerBase
    {
        public class KontaktMitgliedschaftEntry
        {
            public int Id { get; set; }
            public SelectionEntry JuristischePerson { get; set; } = null!;
            public SelectionEntry Mitglied { get; set; } = null!;
            public DateOnly Von { get; set; }
            public DateOnly? Bis { get; set; }
            public decimal? Anteil { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public KontaktMitgliedschaftEntry() { }
            public KontaktMitgliedschaftEntry(KontaktMitgliedschaft entity)
            {
                Id = entity.KontaktMitgliedschaftId;
                JuristischePerson = new(entity.JuristischePerson.KontaktId, entity.JuristischePerson.Bezeichnung);
                Mitglied = new(entity.Mitglied.KontaktId, entity.Mitglied.Bezeichnung);
                Von = entity.Von;
                Bis = entity.Bis;
                Anteil = entity.Anteil;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly SaverwalterContext _ctx;

        public KontaktMitgliedschaftController(SaverwalterContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KontaktMitgliedschaftEntry>>> Get()
        {
            var list = await _ctx.KontaktMitgliedschaften
                .Include(m => m.JuristischePerson)
                .Include(m => m.Mitglied)
                .ToListAsync();
            return Ok(list.Select(m => new KontaktMitgliedschaftEntry(m)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KontaktMitgliedschaftEntry>> Get(int id)
        {
            var entity = await _ctx.KontaktMitgliedschaften
                .Include(m => m.JuristischePerson)
                .Include(m => m.Mitglied)
                .FirstOrDefaultAsync(m => m.KontaktMitgliedschaftId == id);
            if (entity is null) return NotFound();
            return Ok(new KontaktMitgliedschaftEntry(entity));
        }

        [HttpPost]
        public async Task<ActionResult<KontaktMitgliedschaftEntry>> Post([FromBody] KontaktMitgliedschaftEntry entry)
        {
            if (entry.Id != 0) return BadRequest();

            var jp = await _ctx.Kontakte.FindAsync(entry.JuristischePerson.Id);
            if (jp is null) return BadRequest("Juristische Person nicht gefunden.");
            var mitglied = await _ctx.Kontakte.FindAsync(entry.Mitglied.Id);
            if (mitglied is null) return BadRequest("Mitglied nicht gefunden.");

            var entity = new KontaktMitgliedschaft(entry.Von)
            {
                JuristischePerson = jp,
                Mitglied = mitglied,
                Bis = entry.Bis,
                Anteil = entry.Anteil
            };

            _ctx.KontaktMitgliedschaften.Add(entity);
            await _ctx.SaveChangesAsync();
            return Ok(new KontaktMitgliedschaftEntry(entity));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<KontaktMitgliedschaftEntry>> Put(int id, [FromBody] KontaktMitgliedschaftEntry entry)
        {
            var entity = await _ctx.KontaktMitgliedschaften
                .Include(m => m.JuristischePerson)
                .Include(m => m.Mitglied)
                .FirstOrDefaultAsync(m => m.KontaktMitgliedschaftId == id);
            if (entity is null) return NotFound();

            entity.Von = entry.Von;
            entity.Bis = entry.Bis;
            entity.Anteil = entry.Anteil;

            await _ctx.SaveChangesAsync();
            return Ok(new KontaktMitgliedschaftEntry(entity));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _ctx.KontaktMitgliedschaften.FindAsync(id);
            if (entity is null) return NotFound();

            _ctx.KontaktMitgliedschaften.Remove(entity);
            await _ctx.SaveChangesAsync();
            return Ok();
        }
    }
}
