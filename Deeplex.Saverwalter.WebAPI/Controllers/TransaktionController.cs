// Copyright (c) 2023-2025 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/transaktionen")]
    public class TransaktionController : FileControllerBase<TransaktionEntry, Guid, Transaktion>
    {
        public class TransaktionEntryBase
        {
            public Guid Id { get; set; }
            public SelectionEntry? Zahler { get; set; }
            public SelectionEntry? Zahlungsempfaenger { get; set; }
            public DateOnly Zahlungsdatum { get; set; }
            public decimal Betrag { get; set; }
            public string Verwendungszweck { get; set; } = string.Empty;
            public string? Notiz { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public TransaktionEntryBase() { }
            public TransaktionEntryBase(Transaktion entity, Permissions permissions)
            {
                Id = entity.TransaktionId;
                if (entity.Zahler is { } zahler)
                    Zahler = new SelectionEntry(zahler.KontaktId, zahler.Bezeichnung);
                if (entity.Zahlungsempfaenger is { } empfaenger)
                    Zahlungsempfaenger = new SelectionEntry(empfaenger.KontaktId, empfaenger.Bezeichnung);
                Zahlungsdatum = entity.Zahlungsdatum;
                Betrag = entity.Betrag;
                Verwendungszweck = entity.Verwendungszweck;
                Notiz = entity.Notiz;

                Permissions = permissions;
            }
        }

        public class TransaktionEntry : TransaktionEntryBase
        {
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public TransaktionEntry() : base() { }
            public TransaktionEntry(Transaktion entity, Permissions permissions) : base(entity, permissions)
            {
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<TransaktionController> _logger;
        private readonly TransaktionBuchungsService _buchungsService;
        private readonly SaverwalterContext _ctx;
        private readonly IAuthorizationService _auth;
        protected override TransaktionDbService DbService { get; }

        public TransaktionController(
            ILogger<TransaktionController> logger,
            TransaktionDbService dbService,
            TransaktionBuchungsService buchungsService,
            SaverwalterContext ctx,
            IAuthorizationService auth,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _buchungsService = buchungsService;
            _ctx = ctx;
            _auth = auth;
            _logger = logger;
        }

        [HttpGet]
        public Task<PagedResult<TransaktionEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        [HttpPost]
        public Task<ActionResult<TransaktionEntry>> Post([FromBody] TransaktionEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<TransaktionEntry>> Get(Guid id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<TransaktionEntry>> Put(Guid id, [FromBody] TransaktionEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(Guid id) => DbService.Delete(User!, id);

        /// <summary>
        /// Erstellt eine Transaktion mit Buchungssätzen aus typisierten Positionen.
        /// Die Konten werden anhand des Positionstyps und der referenzierten Entitäten
        /// (z.B. Vertrag) automatisch aufgelöst.
        /// </summary>
        [HttpPost("buchen")]
        public async Task<ActionResult<TransaktionEntry>> Buchen(
            [FromBody] TransaktionBuchungsService.TransaktionsInput input)
        {
            foreach (var miete in input.Mieten)
            {
                var vertrag = await _ctx.Vertraege.FindAsync(miete.VertragId);
                if (vertrag is null) return NotFound($"Vertrag {miete.VertragId} nicht gefunden.");
                var authRx = await _auth.AuthorizeAsync(User!, vertrag, [Operations.SubCreate]);
                if (!authRx.Succeeded) return Forbid();
            }

            foreach (var bk in input.BetriebskostenEingaenge)
            {
                var umlage = await _ctx.Umlagen.FindAsync(bk.UmlageId);
                if (umlage is null) return NotFound($"Umlage {bk.UmlageId} nicht gefunden.");
                var authRx = await _auth.AuthorizeAsync(User!, umlage, [Operations.SubCreate]);
                if (!authRx.Succeeded) return Forbid();
            }

            try
            {
                var transaktion = await _buchungsService.BucheAsync(input);
                var permissions = await GetPermissions(User!, transaktion, _auth);
                return Ok(new TransaktionEntry(transaktion, permissions));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}

