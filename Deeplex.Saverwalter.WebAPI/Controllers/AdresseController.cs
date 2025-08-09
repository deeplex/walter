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
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/adressen")]
    public class AdresseController : FileControllerBase<AdresseEntry, int, Adresse>
    {
        public class AdresseEntryBase
        {
            protected Adresse? Entity { get; }

            public int Id { get; set; }

            public string? Anschrift { get; set; }
            public string Strasse { get; set; } = string.Empty;
            public string Hausnummer { get; set; } = string.Empty;
            public string Postleitzahl { get; set; } = string.Empty;
            public string Stadt { get; set; } = string.Empty;

            public Permissions Permissions { get; set; } = new Permissions();

            public AdresseEntryBase() { }
            public AdresseEntryBase(Adresse entity, Permissions permissions)
            {
                Entity = entity;
                Id = Entity.AdresseId;

                Anschrift = entity.Anschrift;
                Strasse = Entity.Strasse;
                Hausnummer = Entity.Hausnummer;
                Postleitzahl = Entity.Postleitzahl;
                Stadt = Entity.Stadt;

                Permissions = permissions;
            }
        }

        public class AdresseEntry : AdresseEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<WohnungEntryBase> Wohnungen { get; set; } = [];
            public IEnumerable<KontaktEntryBase> Kontakte { get; set; } = [];
            public IEnumerable<ZaehlerEntryBase> Zaehler { get; set; } = [];

            public AdresseEntry() : base() { }
            public AdresseEntry(Adresse entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<AdresseController> _logger;
        protected override AdresseDbService DbService { get; }

        public AdresseController(ILogger<AdresseController> logger, AdresseDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<AdresseEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<AdresseEntry>> Post([FromBody] AdresseEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<AdresseEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<AdresseEntry>> Put(int id, AdresseEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
