using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.BetriebskostenrechnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/umlagen/{id}")]
    public class UmlageController : ControllerBase
    {
        public class UmlageEntryBase
        {
            protected Umlage Entity { get; } = null!;

            public int Id { get; set; }
            public string? Notiz { get; set; }
            public string? Beschreibung { get; set; }
            public Umlageschluessel Schluessel { get; set; }
            public Betriebskostentyp Typ { get; set; }

            public string WohnungenBezeichnung => Entity?.GetWohnungenBezeichnung() ?? "";

            protected UmlageEntryBase() { }
            public UmlageEntryBase(Umlage entity)
            {
                Entity = entity;
                Id = Entity.UmlageId;

                Notiz = Entity.Notiz ?? "";
                Beschreibung = Entity.Beschreibung;
                Schluessel = Entity.Schluessel;
                Typ = Entity.Typ;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            private IWalterDbService DbService { get; } = null!;

            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<BetriebskostenrechungListEntry>? Betriebskostenrechnungen => Entity?.Betriebskostenrechnungen.Select(e => new BetriebskostenrechungListEntry(e));
            public IEnumerable<WohnungListEntry>? Wohnungen => Entity?.Wohnungen.Select(e => new WohnungListEntry(e, DbService));
            // TODO Zaehler
            // TODO HKVO

            public UmlageEntry() : base() { }
            public UmlageEntry(Umlage entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<UmlageController> _logger;
        private IWalterDbService DbService { get; }

        public UmlageController(ILogger<UmlageController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "[Controller]")]
        public UmlageEntry Get(int id)
        {
            return new UmlageEntry(DbService.ctx.Umlagen.Find(id), DbService);
        }
    }
}
