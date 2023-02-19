using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.MieteListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.MietminderungListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/vertraege/{id}")]
    public class VertragController : ControllerBase
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
            // TODO Versionen

            public IEnumerable<KontaktListEntry> Mieter => Program.ctx.MieterSet.Where(m => m.Vertrag.VertragId == Entity.VertragId).ToList().Select(e => new KontaktListEntry(Program.ctx.FindPerson(e.PersonId)));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<MieteListEntry> Mieten => Entity.Mieten.Select(e => new MieteListEntry(e));
            public IEnumerable<MietminderungListEntry> Mietminderungen => Entity.Mietminderungen.Select(e => new MietminderungListEntry(e));
            // TODO Garagen

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
