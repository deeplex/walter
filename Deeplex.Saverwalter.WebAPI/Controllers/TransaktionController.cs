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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
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
            public SelectionEntry Zahler { get; set; } = null!;
            public SelectionEntry Zahlungsempfaenger { get; set; } = null!;
            public DateOnly Zahlungsdatum { get; set; }
            public double Betrag { get; set; }
            public string Verwendungszweck { get; set; } = string.Empty;
            public string? Notiz { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public TransaktionEntryBase() { }
            public TransaktionEntryBase(Transaktion entity, Permissions permissions)
            {
                Id = entity.TransaktionId;
                Zahler = new SelectionEntry(entity.Zahler.KontaktId, entity.Zahler.Bezeichnung);
                Zahlungsempfaenger = new SelectionEntry(entity.Zahlungsempfaenger.KontaktId, entity.Zahlungsempfaenger.Bezeichnung);
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
        protected override TransaktionDbService DbService { get; }

        public TransaktionController(
            ILogger<TransaktionController> logger,
            TransaktionDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<TransaktionEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<TransaktionEntry>> Post([FromBody] TransaktionEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<TransaktionEntry>> Get(Guid id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<TransaktionEntry>> Put(Guid id, [FromBody] TransaktionEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(Guid id) => DbService.Delete(User!, id);
    }
}

