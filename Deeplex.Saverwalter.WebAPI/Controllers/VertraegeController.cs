using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VertraegeController : ControllerBase
    {
        public class VertraegeListEntry
        {
            private Vertrag Entity { get; }

            public int id => Entity.VertragId;
            public string Mieter => "TODO";
            public string Wohnung => "TODO";
            public string Beginn => Entity.Beginn().ToString();
            public string Ende => "TODO";

            public VertraegeListEntry(Vertrag v)
            {
                Entity = v;
            }
        }

        private readonly ILogger<VertraegeController> _logger;

        public VertraegeController(ILogger<VertraegeController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetVertraege")]
        public IEnumerable<VertraegeListEntry> Get()
        {
            return Program.ctx.Vertraege.Select(e => new VertraegeListEntry(e)).ToList();
        }
    }
}
