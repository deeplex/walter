using Deeplex.Saverwalter.Model;
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
            private Vertrag Entity { get; }

            public int Id => Entity.VertragId;
            public string Mieter => "TODO"; // Mieterbezeichnung
            public string Wohnung => Entity.Wohnung.Adresse.Anschrift + ", " + Entity.Wohnung.Bezeichnung;
            public string Beginn => Entity.Beginn().Datum();
            public string Ende => Entity.Ende is DateTime d ? d.Datum() : "Offen";

            public VertragListEntry(Vertrag v)
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
        public IEnumerable<VertragListEntry> Get()
        {
            return Program.ctx.Vertraege.Select(e => new VertragListEntry(e)).ToList();
        }
    }
}
