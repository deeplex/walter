﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragController : ControllerBase
    {
        public class VertragEntryBase
        {
            public int Id { get; set; }
            public DateOnly Beginn { get; set; }
            public DateOnly? Ende { get; set; }
            public SelectionEntry? Wohnung { get; set; }
            public string? MieterAuflistung { get; set; }

            // For Tabelle
            public IEnumerable<MieteEntryBase>? Mieten { get; set; }
            public IEnumerable<VertragVersionEntryBase>? Versionen { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public VertragEntryBase() { }
            public VertragEntryBase(Vertrag entity, Permissions permissions)
            {
                Id = entity.VertragId;
                Beginn = entity.Beginn();
                Ende = entity.Ende;
                var anschrift = entity.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Wohnung = new(
                    entity.Wohnung.WohnungId,
                    $"{anschrift} - {entity.Wohnung.Bezeichnung}",
                    entity.Wohnung.Besitzer?.Bezeichnung);

                Mieten = entity.Mieten.ToList().Select(e => new MieteEntryBase(e, permissions));
                Versionen = entity.Versionen.Select(e => new VertragVersionEntryBase(e, permissions));
                MieterAuflistung = string.Join(", ", entity.Mieter.Select(a => a.Bezeichnung));

                Permissions = permissions;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            private Vertrag Entity { get; } = null!;

            public SelectionEntry Ansprechpartner { get; set; } = null!;
            public string? Notiz { get; set; }
            public IEnumerable<SelectionEntry>? SelectedMieter { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public IEnumerable<BetriebskostenrechnungEntryBase> Betriebskostenrechnungen { get; set; } = [];
            public IEnumerable<MietminderungEntryBase> Mietminderungen { get; set; } = [];
            public IEnumerable<KontaktEntryBase> Mieter { get; set; } = [];
            // TODO Garagen

            public VertragEntry() : base() { }
            public VertragEntry(Vertrag entity, Permissions permissions) : base(entity, permissions)
            {
                Entity = entity;

                if (entity.Ansprechpartner is Kontakt a)
                {
                    Ansprechpartner = new(a.KontaktId, a.Bezeichnung);
                }
                Notiz = entity.Notiz;

                SelectedMieter = entity.Mieter.Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung));

                Betriebskostenrechnungen = entity.Wohnung.Umlagen
                    .SelectMany(e => e.Betriebskostenrechnungen)
                    .Where(e => e.BetreffendesJahr >= entity.Beginn().Year && (entity.Ende == null || entity.Ende.Value.Year >= e.BetreffendesJahr))
                    .Select(e => new BetriebskostenrechnungEntryBase(e, permissions));
                Mietminderungen = entity.Mietminderungen.ToList().Select(e => new MietminderungEntryBase(e, permissions));
                Mieter = entity.Mieter.Select(e => new KontaktEntryBase(e, permissions));

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<VertragController> _logger;
        private VertragDbService DbService { get; }

        public VertragController(ILogger<VertragController> logger, VertragDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] VertragEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, VertragEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
