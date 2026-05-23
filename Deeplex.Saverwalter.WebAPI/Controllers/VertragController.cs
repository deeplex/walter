// Copyright (c) 2023-2024 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragController : FileControllerBase<VertragEntry, int, Vertrag>
    {
        public class VertragEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public SelectionEntry? Wohnung { get; set; }
            public string? MieterAuflistung { get; set; }

            // For Tabelle
            public IEnumerable<MieteEntryBase>? Mieten { get; set; }
            public IEnumerable<VertragVersionEntryBase>? Versionen { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity, Permissions permissions)
            {
                Id = entity.VertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;
                if (entity.Wohnung is Wohnung wohnung)
                {
                    var anschrift = wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                    Wohnung = new(
                        wohnung.WohnungId,
                        $"{anschrift} - {wohnung.Bezeichnung}",
                        wohnung.Besitzer?.Bezeichnung);
                }
                else
                {
                    Wohnung = new(0, "Unbekannte Wohnung");
                }

                Mieten = entity.Mieten.ToList().Select(e => new MieteEntryBase(e, permissions));
                Versionen = entity.Versionen.Select(e => new VertragVersionEntryBase(e, permissions));
                MieterAuflistung = string.Join(", ", entity.Mieter.Select(a => a.Bezeichnung));

                Permissions = permissions;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            private Vertrag Entity { get; } = null!;

            public SelectionEntry Ansprechpartner { get; set; } = null!;
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];
            public IEnumerable<MietminderungEntryBase> Mietminderungen { get; set; } = [];
            public IEnumerable<KontaktEntryBase> Mieter { get; set; } = [];

            public IEnumerable<AbrechnungsresultatEntryBase> Abrechnungsresultate { get; set; } = [];
            public IEnumerable<GarageVertragEntryBase> GarageVertraege { get; set; } = [];

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, Permissions permissions) : base(entity, permissions)
            {
                Entity = entity;

                if (entity.Ansprechpartner is Kontakt a)
                {
                    Ansprechpartner = new(a.KontaktId, a.Bezeichnung);
                }
                Notiz = entity.Notiz;

                SelectedMieter = entity.Mieter.Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung));

                Betriebskostenrechnungen = entity.Wohnung.Umlagen
                    .SelectMany(e => e.Betriebskostenrechnungen)
                    .Where(e => e.BetreffendesJahr >= entity.Beginn().Year && (entity.Ende == null || entity.Ende.Value.Year >= e.BetreffendesJahr))
                    .Select(e => new BetriebskostenrechnungEntryBase(e, permissions));
                Mietminderungen = entity.Mietminderungen.ToList().Select(e
                    => new MietminderungEntryBase(e, permissions));
                Mieter = entity.Mieter.Select(e => new KontaktEntryBase(e, permissions));
                Abrechnungsresultate = entity.Abrechnungsresultate.Select(e
                    => new AbrechnungsresultatEntry(e, permissions));
                GarageVertraege = entity.GarageVertraege.Select(e
                    => new GarageVertragEntryBase(e, permissions));

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<VertragController> _logger;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;
        protected override VertragDbService DbService { get; }

        public VertragController(
            ILogger<VertragController> logger,
            VertragDbService dbService,
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
        public Task<PagedResult<VertragEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        public record VertragOverlapInfo(int Id, DateOnly Beginn, DateOnly? Ende, string Mieter);

        [HttpGet("check-overlap")]
        public async Task<ActionResult<VertragOverlapInfo?>> CheckOverlap(
            [FromQuery] int wohnungId,
            [FromQuery] DateOnly beginn,
            [FromQuery] DateOnly? ende,
            [FromQuery] int excludeId = 0)
        {
            var existing = await _ctx.Vertraege
                .Where(v => v.Wohnung.WohnungId == wohnungId && v.VertragId != excludeId)
                .Select(v => new
                {
                    Id = v.VertragId,
                    Ende = v.Ende,
                    Beginn = v.Versionen.Min(vv => (DateOnly?)vv.Beginn),
                    Mieter = v.Mieter.Select(m => m.Bezeichnung)
                })
                .ToListAsync();

            var conflict = existing.FirstOrDefault(v =>
                v.Beginn.HasValue &&
                beginn <= (v.Ende ?? DateOnly.MaxValue) &&
                (ende ?? DateOnly.MaxValue) >= v.Beginn.Value);

            if (conflict is null) return Ok(null);

            return Ok(new VertragOverlapInfo(
                conflict.Id,
                conflict.Beginn!.Value,
                conflict.Ende,
                string.Join(", ", conflict.Mieter)));
        }

        [HttpPost]
        public Task<ActionResult<VertragEntry>> Post([FromBody] VertragEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<VertragEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<VertragEntry>> Put(int id, VertragEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);

        /// <summary>
        /// Returns all Transaktionen that have Buchungssätze touching this Vertrag's
        /// MietBuchungskonto or NkBuchungskonto.
        /// </summary>
        [HttpGet("{id}/transaktionen")]
        public async Task<ActionResult<IEnumerable<TransaktionEntry>>> GetTransaktionen(int id)
        {
            var vertrag = await _ctx.Vertraege.FindAsync(id);
            if (vertrag is null) return NotFound();

            var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.Read]);
            if (!authRx.Succeeded) return Forbid();

            var permissions = await GetPermissions(User!, vertrag, _auth);

            var mietKontoId = vertrag.MietBuchungskonto.BuchungskontoId;
            var nkKontoId = vertrag.NkBuchungskonto.BuchungskontoId;

            var transaktionen = await _ctx.Transaktionen
                .Include(t => t.Zahler)
                .Include(t => t.Zahlungsempfaenger)
                .Include(t => t.Buchungssaetze)
                    .ThenInclude(s => s.Buchungszeilen)
                        .ThenInclude(z => z.Buchungskonto)
                .Where(t => t.Buchungssaetze.Any(s =>
                    s.Buchungszeilen.Any(z =>
                        z.Buchungskonto.BuchungskontoId == mietKontoId ||
                        z.Buchungskonto.BuchungskontoId == nkKontoId)))
                .OrderByDescending(t => t.Zahlungsdatum)
                .ToListAsync();

            var result = transaktionen.Select(t => new TransaktionEntry(t, permissions));
            return Ok(result);
        }
    }
}
