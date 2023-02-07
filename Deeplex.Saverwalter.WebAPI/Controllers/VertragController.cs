using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vertraege/{id}")]
    public class VertragController
    {
        public class VertragEntry
        {
            private Vertrag Entity { get; }

            public int id => Entity.VertragId;
            public DateTime Beginn => Entity.Beginn();
            public DateTime? Ende => Entity.Ende;

            public VertragEntry(Vertrag e)
            {
                Entity = e;
            }
        }

        private readonly ILogger<VertragController> _logger;

        public VertragController(ILogger<VertragController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetVertrag")]
        public VertragEntry Get(int id)
        {
            return new VertragEntry(Program.ctx.Vertraege.Find(id));
        }
    }
}
