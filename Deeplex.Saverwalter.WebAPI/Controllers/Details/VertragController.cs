using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/vertraege/{id}")]
    public class VertragController
    {
        public class VertragEntryBase
        {
            protected Vertrag Entity { get; }

            public int id => Entity.VertragId;
            public DateTime Beginn => Entity.Beginn();
            public DateTime? Ende => Entity.Ende;
            public WohnungEntryBase Wohnung => new WohnungEntryBase(Entity.Wohnung);
            public Guid? AnsprechpartnerId => Entity.AnsprechpartnerId;

            public VertragEntryBase(Vertrag entity)
            {
                Entity = entity;
            }
        }

        public class VertragEntry : VertragEntryBase
        {
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));
            // TODO MietenEntries
            // TODO MietMinderungEntries
            // TODO Mieter?...

            public VertragEntry(Vertrag entity) : base(entity)
            {
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
