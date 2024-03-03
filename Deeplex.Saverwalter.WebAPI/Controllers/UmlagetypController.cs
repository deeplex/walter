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
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagetypen")]
    public class UmlagetypController : FileControllerBase<UmlagetypEntry, Umlagetyp>
    {
        public class UmlagetypEntryBase
        {
            protected Umlagetyp? Entity { get; }
            public int Id { get; set; }
            public string Bezeichnung { get; set; } = null!;

            public Permissions Permissions { get; set; } = new Permissions();

            public UmlagetypEntryBase() { }
            public UmlagetypEntryBase(Umlagetyp entity, Permissions permissions)
            {
                Entity = entity;

                Id = entity.UmlagetypId;
                Bezeichnung = entity.Bezeichnung;

                Permissions = permissions;
            }
        }

        public class UmlagetypEntry : UmlagetypEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<UmlageEntryBase> Umlagen { get; set; } = [];

            public UmlagetypEntry() : base() { }
            public UmlagetypEntry(Umlagetyp entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<UmlagetypController> _logger;
        protected override UmlagetypDbService DbService { get; }

        public UmlagetypController(ILogger<UmlagetypController> logger, UmlagetypDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<UmlagetypEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<UmlagetypEntry>> Post([FromBody] UmlagetypEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<UmlagetypEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<UmlagetypEntry>> Put(int id, UmlagetypEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
