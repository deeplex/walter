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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wohnungen")]
    public class WohnungController : FileControllerBase<WohnungEntry, Wohnung>
    {
        public class WohnungEntryBase
        {
            protected Wohnung? Entity { get; }

            public int Id { get; set; }
            public string Bezeichnung { get; set; } = null!;
            public AdresseEntryBase? Adresse { get; set; }
            public SelectionEntry? Besitzer { get; set; }
            public string? Bewohner { get; set; }
            public double Wohnflaeche { get; set; }
            public double Nutzflaeche { get; set; }
            public double Miteigentumsanteile { get; set; }
            public int Einheiten { get; set; }
 
            public Permissions Permissions { get; set; } = new Permissions();

            public WohnungEntryBase() { }
            public WohnungEntryBase(Wohnung entity, Permissions permissions)
            {
                Entity = entity;

                Id = entity.WohnungId;
                Bezeichnung = entity.Bezeichnung;
                Adresse = entity.Adresse is Adresse a ? new AdresseEntryBase(a, permissions) : null;
                if (entity.Besitzer is Kontakt k)
                {
                    Besitzer = new(k.KontaktId, k.Bezeichnung);
                }

                Wohnflaeche = entity.Wohnflaeche;
                Nutzflaeche = entity.Nutzflaeche;
                Miteigentumsanteile = entity.Miteigentumsanteile;
                Einheiten = entity.Nutzeinheit;

                var v = entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateOnly.FromDateTime(DateTime.Now));
                Bewohner = v != null ?
                    string.Join(", ", v.Mieter.Select(m => m.Bezeichnung)) :
                    null;

                Permissions = permissions;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
           public string? Notiz { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<WohnungEntryBase> Haus { get; set; } = [];
            public IEnumerable<ZaehlerEntryBase> Zaehler { get; } = [];
            public IEnumerable<VertragEntryBase> Vertraege { get; } = [];
            public IEnumerable<ErhaltungsaufwendungEntryBase> Erhaltungsaufwendungen { get; } = [];
            public IEnumerable<UmlageEntryBase> Umlagen { get; } = [];
            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; } = [];

            public WohnungEntry() : base() { }
            public WohnungEntry(Wohnung entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;

                Zaehler = entity.Zaehler.Select(e => new ZaehlerEntryBase(e, permissions));
                Vertraege = entity.Vertraege.Select(e => new VertragEntryBase(e, permissions));
                Erhaltungsaufwendungen = entity.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungEntryBase(e, permissions));
                Umlagen = entity.Umlagen.Select(e => new UmlageEntryBase(e, permissions));
                Betriebskostenrechnungen = entity.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechnungEntryBase(f, permissions)));
            }
        }

        private readonly ILogger<WohnungController> _logger;
        protected override WohnungDbService DbService { get; }

        public WohnungController(ILogger<WohnungController> logger, WohnungDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<WohnungEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        [Authorize(Policy = "RequireOwner")]
        public Task<ActionResult<WohnungEntry>> Post([FromBody] WohnungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<WohnungEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<WohnungEntry>> Put(int id, WohnungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
