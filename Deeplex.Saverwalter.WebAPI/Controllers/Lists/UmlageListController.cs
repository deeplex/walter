using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;

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

        public UmlageListController(ILogger<UmlageListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetUmlageList")]
        public IEnumerable<UmlageListEntry> Get()
        {
            return Program.ctx.Umlagen
                .Include(e => e.Wohnungen)
                .ThenInclude(e => e.Adresse)
                .Select(e => new UmlageListEntry(e)).ToList();
        }
    }
}
