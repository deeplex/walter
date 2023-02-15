using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/vertraege")]
    public class VertragListController : ControllerBase
    {
        public class VertraegeListEntry
        {
            private Vertrag Entity { get; }

            public int Id => Entity.VertragId;
            public string Mieter => "TODO"; // Mieterbezeichnung
            public string Wohnung => Entity.Wohnung.Adresse.Anschrift + ", " + Entity.Wohnung.Bezeichnung;
            public string Beginn => Entity.Beginn().Datum();
            public string Ende => Entity.Ende is DateTime d ? d.Datum() : "Offen";

            public VertraegeListEntry(Vertrag v)
            {
                Entity = v;
            }
        }

        private readonly ILogger<VertragListController> _logger;

        public VertragListController(ILogger<VertragListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetVertragList")]
        public IEnumerable<VertraegeListEntry> Get()
        {
            return Program.ctx.Vertraege.Include(e => e.Versionen).Include(e => e.Wohnung).ThenInclude(w => w.Adresse).Select(e => new VertraegeListEntry(e)).ToList();
        }
    }
}
