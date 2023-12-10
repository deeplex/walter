﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte")]
    public class KontaktController : ControllerBase
    {
        public class KontaktEntryBase
        {
            protected Kontakt? Entity { get; }

            public int Id { get; set; }
            public SelectionEntry? Anrede { get; set; }
            public SelectionEntry Rechtsform { get; set; } = null!;
            public string? Vorname { get; set; }
            public string Bezeichnung { get; } = null!;
            public string Name { get; set; } = null!;
            public string? Email { get; set; }
            public string? Fax { get; set; }
            public string? Notiz { get; set; }
            public string? Telefon { get; set; }
            public string? Mobil { get; set; }
            public AdresseEntryBase? Adresse { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            protected KontaktEntryBase() : base() { }
            public KontaktEntryBase(Kontakt entity, Permissions permissions)
            {
                Entity = entity;
                Id = entity.KontaktId;
                Anrede = new SelectionEntry((int)Entity.Anrede, Entity.Anrede.ToString());
                Rechtsform = new SelectionEntry((int)Entity.Rechtsform, Entity.Rechtsform.ToDescriptionString());
                Vorname = Entity.Vorname;
                Name = Entity.Name;
                Bezeichnung = Entity.Bezeichnung;
                Email = Entity.Email;
                Fax = Entity.Fax;
                Notiz = Entity.Notiz;
                Telefon = Entity.Telefon;
                Mobil = Entity.Mobil;

                if (Entity.Adresse is Adresse a)
                {
                    Adresse = new AdresseEntryBase(a, permissions);
                }

                CreatedAt = Entity.CreatedAt;
                LastModified = Entity.LastModified;

                Permissions = permissions;
            }
        }

        public sealed class KontaktEntry : KontaktEntryBase
        {
            public IEnumerable<SelectionEntry>? SelectedJuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<SelectionEntry>? SelectedMitglieder
                => Entity?.Mitglieder.Select(e => new SelectionEntry(e.KontaktId, e.Name));

            public IEnumerable<KontaktEntryBase>? JuristischePersonen
                => Entity?.JuristischePersonen.Select(e => new KontaktEntryBase(e, new()));

            public IEnumerable<KontaktEntryBase>? Mitglieder
                => Entity?.Mitglieder.Select(e => new KontaktEntryBase(e, new()));

            public IEnumerable<VertragEntryBase>? Vertraege
                => Entity?.Mietvertraege
                .Concat(Entity.Wohnungen.SelectMany(w => w.Vertraege))
                .Distinct()
                .Select(e => new VertragEntryBase(e, new()));

            public IEnumerable<WohnungEntryBase>? Wohnungen
                => Entity?.Mietvertraege
                .Concat(Entity.Wohnungen.SelectMany(w => w.Vertraege))
                .Select(e => e.Wohnung)
                .Distinct()
                .Select(e => new WohnungEntryBase(e, new()));

            public KontaktEntry() : base() { }
            public KontaktEntry(Kontakt entity, Permissions permissions) : base(entity, permissions) { }
        }

        private readonly ILogger<KontaktController> _logger;
        private KontaktDbService DbService { get; }

        public KontaktController(ILogger<KontaktController> logger, KontaktDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet]
        public Task<IActionResult> Get() => DbService.GetList();
        [HttpPost]
        public Task<IActionResult> Post([FromBody] KontaktEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, KontaktEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
