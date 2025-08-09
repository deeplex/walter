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
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietminderungen")]
    public class MietminderungController : FileControllerBase<MietminderungEntry, int, Mietminderung>
    {
        public class MietminderungEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public double Minderung { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public MietminderungEntryBase() { }
            public MietminderungEntryBase(Mietminderung entity, Permissions permissions)
            {
                Id = entity.MietminderungId;
                Beginn = entity.Beginn;
                Ende = entity.Ende;
                Minderung = entity.Minderung;

                Permissions = permissions;
            }
        }

        public class MietminderungEntry : MietminderungEntryBase
        {
            public string? Notiz { get; set; }
            public SelectionEntry Vertrag { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public MietminderungEntry() : base() { }
            public MietminderungEntry(Mietminderung entity, Permissions permissions) : base(entity, permissions)
            {
                var anschrift = entity.Vertrag.Wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                var vertragTitle = $"{anschrift} - {entity.Vertrag.Wohnung.Bezeichnung} - {Beginn.Datum()}";
                Notiz = entity.Notiz;
                Vertrag = new(entity.Vertrag.VertragId, vertragTitle);

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<MietminderungController> _logger;
        protected override MietminderungDbService DbService { get; }

        public MietminderungController(ILogger<MietminderungController> logger, MietminderungDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<MietminderungEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<MietminderungEntry>> Post([FromBody] MietminderungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<MietminderungEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<MietminderungEntry>> Put(int id, [FromBody] MietminderungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
