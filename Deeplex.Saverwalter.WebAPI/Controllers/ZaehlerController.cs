﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/zaehler")]
    public class ZaehlerController : ControllerBase
    {
        public class ZaehlerEntryBase
        {
            protected Zaehler? Entity { get; }

            public int Id { get; set; }
            public string Kennnummer { get; set; } = null!;
            public AdresseEntryBase? Adresse { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry Typ { get; set; } = null!;
            public SelectionEntry? Wohnung { get; set; }
            public ZaehlerstandEntryBase? LastZaehlerstand { get; set; }
            public IEnumerable<SelectionEntry>? SelectedUmlagen { get; set; }
            public DateOnly? Ende { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public ZaehlerEntryBase() { }
            public ZaehlerEntryBase(Zaehler entity)
            {
                Entity = entity;

                Id = Entity.ZaehlerId;
                Kennnummer = Entity.Kennnummer;
                Typ = new((int)Entity.Typ, Entity.Typ.ToString());
                Adresse = Entity.Adresse is Adresse a ? new AdresseEntryBase(a) : null;
                Wohnung = Entity.Wohnung is Wohnung w ? new(w.WohnungId, $"{w.Adresse?.Anschrift ?? "Unbekannte Anschrift"}, {w.Bezeichnung}") : null;
                Notiz = Entity.Notiz;
                var letzterStand = Entity.Staende?.OrderBy(s => s.Datum).ToList().LastOrDefault();
                Ende = Entity.Ende;
                if (letzterStand is Zaehlerstand stand)
                {
                    LastZaehlerstand = new ZaehlerstandEntryBase(letzterStand);
                }

                SelectedUmlagen = entity.Umlagen.ToList()
                   .Select(e => new SelectionEntry(e.UmlageId, e.Typ.Bezeichnung + " - " + e.GetWohnungenBezeichnung()));

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            public IEnumerable<ZaehlerstandEntryBase>? Staende => Entity?.Staende.ToList().Select(e => new ZaehlerstandEntryBase(e));
            public IEnumerable<UmlageEntryBase>? Umlagen => Entity?.Umlagen.ToList().Select(e => new UmlageEntryBase(e));

            public ZaehlerEntry() : base() { }
            public ZaehlerEntry(Zaehler entity) : base(entity)
            {
            }
        }

        private readonly ILogger<ZaehlerController> _logger;
        private ZaehlerDbService DbService { get; }

        public ZaehlerController(ILogger<ZaehlerController> logger, ZaehlerDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] ZaehlerEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, ZaehlerEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
