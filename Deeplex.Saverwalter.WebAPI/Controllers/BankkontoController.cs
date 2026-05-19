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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BankkontoController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/bankkontos")]
    public class BankkontoController : FileControllerBase<BankkontoEntry, int, Bankkonto>
    {
        public class BankkontoEntryBase
        {
            protected Bankkonto? Entity { get; }

            public int Id { get; set; }
            public string? Bank { get; set; }
            public string? Iban { get; set; }
            public string Bezeichnung { get; set; } = null!;

            public Permissions Permissions { get; set; } = new Permissions();

            public BankkontoEntryBase() { }
            public BankkontoEntryBase(Bankkonto entity, Permissions permissions)
            {
                Entity = entity;
                Id = entity.BankkontoId;
                Bank = entity.Bank;
                Iban = entity.Iban;
                Bezeichnung = entity.Iban ?? entity.Bank ?? $"Bankkonto {entity.BankkontoId}";
                Permissions = permissions;
            }
        }

        public sealed class BankkontoEntry : BankkontoEntryBase
        {
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry> SelectedBesitzer { get; set; } = [];
            public IEnumerable<TransaktionEntryBase> Transaktionen { get; set; } = [];
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public BankkontoEntry() { }
            public BankkontoEntry(Bankkonto entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                SelectedBesitzer = entity.Besitzer.Select(k => new SelectionEntry(k.KontaktId, k.Bezeichnung));
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<BankkontoController> _logger;
        protected override BankkontoDbService DbService { get; }

        public BankkontoController(ILogger<BankkontoController> logger, BankkontoDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<PagedResult<BankkontoEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);
        [HttpPost]
        public Task<ActionResult<BankkontoEntry>> Post([FromBody] BankkontoEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<BankkontoEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<BankkontoEntry>> Put(int id, BankkontoEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
