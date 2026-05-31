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
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/garagen")]
    public class GarageController : FileControllerBase<GarageEntry, int, Garage>
    {
        public class GarageEntryBase
        {
            public int Id { get; set; }
            public string Kennung { get; set; } = null!;
            public SelectionEntry? Besitzer { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public string? AktuelleMieter { get; set; }
            public Permissions Permissions { get; set; } = new Permissions();

            public GarageEntryBase() { }
            public GarageEntryBase(Garage entity, Permissions permissions)
            {
                Id = entity.GarageId;
                Kennung = entity.Kennung;
                if (entity.Besitzer is Kontakt k)
                {
                    Besitzer = new(k.KontaktId, k.Bezeichnung);
                }
                Adresse = entity.Adresse is Adresse a ? new AdresseEntryBase(a, permissions) : null;

                var today = DateOnly.FromDateTime(DateTime.Now);
                var aktuellerVertrag = entity.Vertraege
                    .FirstOrDefault(v => v.Ende == null || v.Ende >= today);
                AktuelleMieter = aktuellerVertrag != null
                    ? GetMieterText(aktuellerVertrag)
                    : null;

                Permissions = permissions;
            }

            protected static string GetMieterText(GarageVertrag gv)
            {
                if (gv.Mieter.Count > 0)
                    return string.Join(", ", gv.Mieter.Select(m => m.Bezeichnung));
                if (gv.Vertrag is Vertrag v)
                    return string.Join(", ", v.Mieter.Select(m => m.Bezeichnung));
                return string.Empty;
            }
        }

        public class GarageEntry : GarageEntryBase
        {
            public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public IEnumerable<GarageVertragEntryBase> Vertraege { get; set; } = [];

            public GarageEntry() : base() { }
            public GarageEntry(Garage entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
                Vertraege = entity.Vertraege.Select(v => new GarageVertragEntryBase(v, permissions));
            }
        }

        private readonly ILogger<GarageController> _logger;
        protected override GarageDbService DbService { get; }

        public GarageController(
            ILogger<GarageController> logger,
            GarageDbService dbService,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<PagedResult<GarageEntryBase>> Get([FromQuery] PagedQuery query)
            => DbService.GetList(User!, query);

        [HttpPost]
        [Authorize(Policy = "RequireOwner")]
        public Task<ActionResult<GarageEntry>> Post([FromBody] GarageEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<GarageEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<GarageEntry>> Put(int id, GarageEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
