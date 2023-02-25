using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragListController : ControllerBase
    {
        public class VertragListEntry
        {
            private IWalterDbService DbService { get; }
            private Vertrag Entity { get; }

            public int Id => Entity.VertragId;
            public string Mieter => string.Join(", ", DbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == Entity.VertragId).ToList().Select(a => DbService.ctx.FindPerson(a.PersonId).Bezeichnung));
            public string Wohnung => Entity.Wohnung.Adresse.Anschrift + ", " + Entity.Wohnung.Bezeichnung;
            public string Beginn => Entity.Beginn().Datum();
            public string Ende => Entity.Ende is DateTime d ? d.Datum() : "Offen";

            public VertragListEntry(Vertrag v, IWalterDbService dbService)
            {
                DbService = dbService;
                Entity = v;
            }
        }

        private readonly ILogger<VertragListController> _logger;
        private IWalterDbService DbService { get; }

        public VertragListController(ILogger<VertragListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "GetVertragList")]
        public IEnumerable<VertragListEntry> Get()
        {
            return DbService.ctx.Vertraege.Select(e => new VertragListEntry(e, DbService)).ToList();
        }
    }
}
