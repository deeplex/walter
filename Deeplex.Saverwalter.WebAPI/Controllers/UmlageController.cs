﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/umlagen")]
    public class UmlageController : ControllerBase
    {
        public class UmlageEntryBase
        {
            protected Umlage? Entity { get; }

            public int? Id { get; set; }
            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public SelectionEntry? Schluessel { get; set; }
            public SelectionEntry? Typ { get; set; }
            public IEnumerable<SelectionEntry>? SelectedWohnungen { get; set; }
            public string? WohnungenBezeichnung { get; set; }

            protected UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity)
            {
                Entity = entity;
                Id = Entity.UmlageId;

                Notiz = Entity.Notiz;
                Beschreibung = Entity.Beschreibung;
                Schluessel = new SelectionEntry((int)Entity.Schluessel, Entity.Schluessel.ToDescriptionString());
                Typ = new SelectionEntry((int)Entity.Typ, Entity.Typ.ToDescriptionString());
                WohnungenBezeichnung = Entity.GetWohnungenBezeichnung() ?? "";
                SelectedWohnungen = Entity.Wohnungen.Select(e => new SelectionEntry(e.WohnungId, e.Adresse.Anschrift + " - " + e.Bezeichnung));
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private WalterDbService.WalterDb? DbService { get; }

            public IEnumerable<BetriebskostenrechnungEntryBase>? Betriebskostenrechnungen => Entity?.Betriebskostenrechnungen
                .Select(e => new BetriebskostenrechnungEntryBase(e));
            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, DbService!));
            // TODO Zaehler
            // TODO HKVO

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, WalterDbService.WalterDb dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<UmlageController> _logger;
        private UmlageDbService DbService { get; }

        public UmlageController(ILogger<UmlageController> logger, UmlageDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get() => new OkObjectResult(DbService.ctx.Umlagen.ToList().Select(e => new UmlageEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] UmlageEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, UmlageEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
