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
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungskontoController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/garage-vertraege")]
    public class GarageVertragController : FileControllerBase<GarageVertragEntry, int, GarageVertrag>
    {
        public class GarageVertragVersionEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public decimal GaragenMiete { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public GarageVertragVersionEntryBase() { }
            public GarageVertragVersionEntryBase(GarageVertragVersion entity, Permissions permissions)
            {
                Id = entity.GarageVertragVersionId;
                Beginn = entity.Beginn;
                GaragenMiete = entity.GaragenMiete;
                Permissions = permissions;
            }
        }

        public class GarageVertragEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public SelectionEntry? Garage { get; set; }
            public SelectionEntry? Vertrag { get; set; }
            public string? MieterAuflistung { get; set; }
            public IEnumerable<GarageVertragVersionEntryBase>? Versionen { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public GarageVertragEntryBase() { }
            public GarageVertragEntryBase(GarageVertrag entity, Permissions permissions)
            {
                Id = entity.GarageVertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;

                Garage = new(entity.Garage.GarageId, entity.Garage.Kennung);

                if (entity.Vertrag is Vertrag v)
                {
                    var anschrift = v.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                    Vertrag = new(v.VertragId, $"{anschrift} - {v.Wohnung.Bezeichnung}");
                }

                if (entity.Mieter.Count > 0)
                    MieterAuflistung = string.Join(", ", entity.Mieter.Select(m => m.Bezeichnung));
                else if (entity.Vertrag is Vertrag linkedVertrag)
                    MieterAuflistung = string.Join(", ", linkedVertrag.Mieter.Select(m => m.Bezeichnung));

                Versionen = entity.Versionen.Select(v => new GarageVertragVersionEntryBase(v, permissions));
                Permissions = permissions;
            }
        }

        public class GarageVertragEntry : GarageVertragEntryBase
        {
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }
            public IEnumerable<KontaktEntryBase> Mieter { get; set; } = [];
            public IEnumerable<BuchungskontoRefEntry> Konten { get; set; } = [];
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public GarageVertragEntry() : base() { }
            public GarageVertragEntry(GarageVertrag entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                SelectedMieter = entity.Mieter.Select(m => new SelectionEntry(m.KontaktId, m.Bezeichnung));
                Mieter = entity.Mieter.Select(m => new KontaktEntryBase(m, permissions));
                Konten = BuchungskontoRefEntry.Collect(
                    (entity.MietBuchungskonto, KontoFunktion.Mietforderungen),
                    (entity.ZahlungsKonto, KontoFunktion.Zahlungseingaenge));
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<GarageVertragController> _logger;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;
        protected override GarageVertragDbService DbService { get; }

        public GarageVertragController(
            ILogger<GarageVertragController> logger,
            GarageVertragDbService dbService,
            SaverwalterContext ctx,
            IAuthorizationService auth,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _ctx = ctx;
            _auth = auth;
            _logger = logger;
        }

        [HttpGet]
        public Task<PagedResult<GarageVertragEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        public record GarageVertragOverlapInfo(int Id, DateOnly Beginn, DateOnly? Ende, string Mieter);

        [HttpGet("check-overlap")]
        public async Task<ActionResult<GarageVertragOverlapInfo?>> CheckOverlap(
            [FromQuery] int garageId,
            [FromQuery] DateOnly beginn,
            [FromQuery] DateOnly? ende,
            [FromQuery] int excludeId = 0)
        {
            var existing = await _ctx.GarageVertraege
                .Where(v => v.Garage.GarageId == garageId && v.GarageVertragId != excludeId)
                .Select(v => new
                {
                    Id = v.GarageVertragId,
                    Ende = v.Ende,
                    Beginn = v.Versionen.Min(vv => (DateOnly?)vv.Beginn),
                    Mieter = v.Mieter.Select(m => m.Bezeichnung),
                    VertragMieter = v.Vertrag != null ? v.Vertrag.Mieter.Select(m => m.Bezeichnung) : Enumerable.Empty<string>()
                })
                .ToListAsync();

            var conflict = existing.FirstOrDefault(v =>
                v.Beginn.HasValue &&
                beginn <= (v.Ende ?? DateOnly.MaxValue) &&
                (ende ?? DateOnly.MaxValue) >= v.Beginn.Value);

            if (conflict is null) return Ok(null);

            var mieterText = conflict.Mieter.Any()
                ? string.Join(", ", conflict.Mieter)
                : string.Join(", ", conflict.VertragMieter);

            return Ok(new GarageVertragOverlapInfo(
                conflict.Id,
                conflict.Beginn!.Value,
                conflict.Ende,
                mieterText));
        }

        [HttpPost]
        public Task<ActionResult<GarageVertragEntry>> Post([FromBody] GarageVertragEntry entry)
            => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<GarageVertragEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<GarageVertragEntry>> Put(int id, GarageVertragEntry entry)
            => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);

        public class GarageForderungsstatusEntry
        {
            public int GarageVertragId { get; set; }
            public string GarageKennung { get; set; } = string.Empty;
            public decimal GaragenMiete { get; set; }
            public decimal SchonGezahlt { get; set; }
            public decimal VerbleibendeForderung { get; set; }
            public bool SollstellungVorhanden { get; set; }
        }

        [HttpGet("{id}/forderung/{monat}")]
        public async Task<ActionResult<GarageForderungsstatusEntry>> GetForderungsstatus(int id, DateOnly monat)
        {
            var gv = await _ctx.GarageVertraege
                .Include(g => g.Versionen)
                .Include(g => g.Garage)
                .Include(g => g.MietBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .FirstOrDefaultAsync(g => g.GarageVertragId == id);

            if (gv is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User, gv, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var ersterDesMonats = new DateOnly(monat.Year, monat.Month, 1);
            var version = gv.Versionen.Where(v => v.Beginn <= ersterDesMonats).MaxBy(v => v.Beginn);
            if (version is null) return NotFound();

            var soll = gv.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll
                    && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                    && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                .Sum(z => z.Betrag);

            var haben = gv.MietBuchungskonto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben
                    && z.Buchungssatz.Buchungsdatum.Year == ersterDesMonats.Year
                    && z.Buchungssatz.Buchungsdatum.Month == ersterDesMonats.Month)
                .Sum(z => z.Betrag);

            return Ok(new GarageForderungsstatusEntry
            {
                GarageVertragId = gv.GarageVertragId,
                GarageKennung = gv.Garage.Kennung,
                GaragenMiete = version.GaragenMiete,
                SchonGezahlt = haben,
                VerbleibendeForderung = soll - haben,
                SollstellungVorhanden = soll > 0
            });
        }
    }
}
