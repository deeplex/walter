using Deeplex.Saverwalter.Model;
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
            protected Umlage Entity { get; }

            public int Id => Entity.UmlageId;
            public string Notiz => Entity.Notiz ?? "";
            public string WohnungenBezeichnung => Entity.GetWohnungenBezeichnung();
            public string? Beschreibung => Entity.Beschreibung;
            public Umlageschluessel Schluessel => Entity.Schluessel;
            public Betriebskostentyp Typ => Entity.Typ;

            public UmlageEntryBase(Umlage entity)
            {
                Entity = entity;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<BetriebskostenrechungListEntry> Betriebskostenrechnungen => Entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechungListEntry(e));
            public IEnumerable<WohnungListEntry> Wohnungen => Entity.Wohnungen.Select(e => new WohnungListEntry(e));
            // TODO Zaehler
            // TODO HKVO

            public UmlageEntry(Umlage entity) : base(entity)
            {
            }
        }

        private readonly ILogger<UmlageController> _logger;

        public UmlageController(ILogger<UmlageController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetUmlage")]
        public UmlageEntry Get(int id)
        {
            return new UmlageEntry(Program.ctx.Umlagen.Find(id));
        }
    }
}
