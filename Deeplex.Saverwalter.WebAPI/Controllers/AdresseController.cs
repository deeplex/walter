﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/adressen")]
    public class AdresseController : ControllerBase
    {
        public class AdresseEntryBase
        {
            protected Adresse? Entity { get; }

            public int Id { get; set; }
            public string? Strasse { get; set; }
            public string? Hausnummer { get; set; }
            public string? Postleitzahl { get; set; }
            public string? Stadt { get; set; }
            public string? Anschrift { get; set; }

            protected AdresseEntryBase() { }
            public AdresseEntryBase(Adresse entity)
            {
                Entity = entity;
                Id = Entity.AdresseId;
                
                Strasse= Entity.Strasse;
                Hausnummer = Entity.Hausnummer;
                Postleitzahl   = Entity.Postleitzahl;
                Stadt = Entity.Stadt;
                Anschrift = Entity.Anschrift;
            }
        }

        public class AdresseEntry : AdresseEntryBase
        {
            private IWalterDbService? DbService { get; }

            public IEnumerable<WohnungEntryBase>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungEntryBase(e, DbService!));

            public AdresseEntry() : base() { }
            public AdresseEntry(Adresse entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<AdresseController> _logger;
        AdresseDbService DbService { get; }

        public AdresseController(ILogger<AdresseController> logger, AdresseDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.ctx.Adressen
            .ToList()
            .Select(e => new AdresseEntryBase(e))
            .ToList());
        [HttpPost]
        public IActionResult Post([FromBody] AdresseEntry entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, AdresseEntry entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
