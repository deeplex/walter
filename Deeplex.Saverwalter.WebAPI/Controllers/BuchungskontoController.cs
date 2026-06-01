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

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/buchungskonten")]
    public class BuchungskontoController(SaverwalterContext ctx) : ControllerBase
    {
        public class BuchungskontoEntry
        {
            public int Id { get; set; }
            public string Kontonummer { get; set; } = "";
            public string Bezeichnung { get; set; } = "";
            public string Kontotyp { get; set; } = "";
            public string? Notiz { get; set; }
            public int AnzahlBuchungszeilen { get; set; }
            public decimal Saldo { get; set; }
        }

        public class BuchungskontoDetail : BuchungskontoEntry
        {
            public List<BuchungszeileInfo> LetzteZeilen { get; set; } = [];
        }

        public class BuchungszeileInfo
        {
            public Guid Id { get; set; }
            public DateOnly Datum { get; set; }
            public string Beschreibung { get; set; } = "";
            public string SollHaben { get; set; } = "";
            public decimal Betrag { get; set; }
        }

        public class BuchungskontoUpdateEntry
        {
            public string Bezeichnung { get; set; } = "";
            public string? Notiz { get; set; }
        }

        private static BuchungskontoEntry ToEntry(Buchungskonto k) => new()
        {
            Id = k.BuchungskontoId,
            Kontonummer = k.Kontonummer,
            Bezeichnung = k.Bezeichnung,
            Kontotyp = k.Kontotyp.ToString(),
            Notiz = k.Notiz,
            AnzahlBuchungszeilen = k.Buchungszeilen.Count,
            Saldo = k.Buchungszeilen
                .Sum(z => z.SollHaben == Model.SollHaben.Soll ? z.Betrag : -z.Betrag)
        };

        /// <summary>
        /// Buchungskonten, die der Nutzer in der gegebenen Rolle sehen darf — Admin
        /// alle, sonst nur Konten von Wohnungen (inkl. Verträge/Garagen/Umlagen), die
        /// er verwaltet. Das Buchungskonto ist der Anker der Buchungssätze, daher wird
        /// die Sichtbarkeit hier verankert.
        /// </summary>
        private IQueryable<Buchungskonto> ScopedKonten(VerwalterRolle rolle)
        {
            if (User.IsInRole("Admin"))
            {
                return ctx.Buchungskonten;
            }

            var kontoIds = TransaktionPermissionHandler
                .ManagedBuchungskontoIds(ctx, User.GetUserId(), rolle);
            return ctx.Buchungskonten.Where(k => kontoIds.Contains(k.BuchungskontoId));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BuchungskontoEntry>>> GetAll()
        {
            var konten = await ScopedKonten(VerwalterRolle.Keine)
                .Include(k => k.Buchungszeilen)
                .OrderBy(k => k.Kontonummer)
                .ToListAsync();

            return Ok(konten.Select(ToEntry));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BuchungskontoDetail>> Get(int id)
        {
            var konto = await ScopedKonten(VerwalterRolle.Keine)
                .Include(k => k.Buchungszeilen)
                    .ThenInclude(z => z.Buchungssatz)
                .FirstOrDefaultAsync(k => k.BuchungskontoId == id);

            if (konto is null)
            {
                // Existiert das Konto, ist aber außerhalb des Sichtbereichs? -> 403 statt 404
                return await ctx.Buchungskonten.AnyAsync(k => k.BuchungskontoId == id)
                    ? Forbid()
                    : NotFound();
            }

            var entry = ToEntry(konto);
            var detail = new BuchungskontoDetail
            {
                Id = entry.Id,
                Kontonummer = entry.Kontonummer,
                Bezeichnung = entry.Bezeichnung,
                Kontotyp = entry.Kontotyp,
                Notiz = entry.Notiz,
                AnzahlBuchungszeilen = entry.AnzahlBuchungszeilen,
                Saldo = entry.Saldo,
                LetzteZeilen = konto.Buchungszeilen
                    .OrderByDescending(z => z.Buchungssatz.Buchungsdatum)
                    .Take(50)
                    .Select(z => new BuchungszeileInfo
                    {
                        Id = z.BuchungszeileId,
                        Datum = z.Buchungssatz.Buchungsdatum,
                        Beschreibung = z.Buchungssatz.Beschreibung,
                        SollHaben = z.SollHaben == Model.SollHaben.Soll ? "Soll" : "Haben",
                        Betrag = z.Betrag
                    })
                    .ToList()
            };

            return Ok(detail);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BuchungskontoEntry>> Put(int id, [FromBody] BuchungskontoUpdateEntry update)
        {
            var konto = await ScopedKonten(VerwalterRolle.Vollmacht)
                .Include(k => k.Buchungszeilen)
                .FirstOrDefaultAsync(k => k.BuchungskontoId == id);

            if (konto is null)
            {
                return await ctx.Buchungskonten.AnyAsync(k => k.BuchungskontoId == id)
                    ? Forbid()
                    : NotFound();
            }

            konto.Bezeichnung = update.Bezeichnung;
            konto.Notiz = update.Notiz;
            await ctx.SaveChangesAsync();

            return Ok(ToEntry(konto));
        }
    }
}
