using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/umlagen")]
    public class UmlageListController : ControllerBase
    {
        public class UmlageListEntry
        {
            private Umlage Entity { get; }

            public int id => Entity.UmlageId;
            public Betriebskostentyp Typ => Entity.Typ;
            public string Wohnungen => Entity.Wohnungen.GetWohnungenBezeichnung();
            
            public UmlageListEntry(Umlage u)
            {
                Entity = u;
            }
        }

        private readonly ILogger<UmlageListController> _logger;
        private IWalterDbService DbService { get; }

        public UmlageListController(ILogger<UmlageListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "GetUmlageList")]
        public IEnumerable<UmlageListEntry> Get()
        {
            return DbService.ctx.Umlagen.Select(e => new UmlageListEntry(e)).ToList();
        }
    }
}
