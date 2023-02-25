using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/wohnungen")]
    public class WohnungListController : ControllerBase
    {
        public class WohnungListEntry
        {
            private Wohnung Entity { get; }
            private IWalterDbService DbService { get; }

            public int id => Entity.WohnungId;
            public string Bezeichnung => Entity.Bezeichnung;
            public string Besitzer => DbService.ctx.FindPerson(Entity.BesitzerId) is IPerson p ? p.Bezeichnung : "Unbekannt";
            private Vertrag? Vertrag => Entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateTime.Now);
            public string? Bewohner => Vertrag is Vertrag v ? string.Join(", ", DbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == v.VertragId).ToList().Select(a => DbService.ctx.FindPerson(a.PersonId).Bezeichnung)) : "Keine";

            public string Anschrift => Entity.Adresse.Anschrift;

            public WohnungListEntry(Wohnung w, IWalterDbService dbService)
            {
                DbService = dbService;
                Entity = w;
            }
        }

        private readonly ILogger<WohnungListController> _logger;
        private IWalterDbService DbService { get; }

        public WohnungListController(ILogger<WohnungListController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "[Controller]")]
        public IEnumerable<WohnungListEntry> Get()
        {
            return DbService.ctx.Wohnungen.Select(e => new WohnungListEntry(e, DbService)).ToList();
        }
    }
}
