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

            public int id => Entity.WohnungId;
            public string Bezeichnung => Entity.Bezeichnung;
            public string Besitzer => Program.ctx.FindPerson(Entity.BesitzerId) is IPerson p ? p.Bezeichnung : "Unbekannt";
            private Vertrag? Vertrag => Entity.Vertraege.FirstOrDefault(e => e.Ende == null || e.Ende < DateTime.Now);
            public string? Bewohner => Vertrag is Vertrag v ? string.Join(", ", Program.ctx.MieterSet.Where(m => m.Vertrag.VertragId == v.VertragId).ToList().Select(a => Program.ctx.FindPerson(a.PersonId).Bezeichnung)) : "Keine";

            public string Anschrift => Entity.Adresse.Anschrift;

            public WohnungListEntry(Wohnung w)
            {
                Entity = w;
            }
        }

        private readonly ILogger<WohnungListController> _logger;

        public WohnungListController(ILogger<WohnungListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWohnungList")]
        public IEnumerable<WohnungListEntry> Get()
        {
            return Program.ctx.Wohnungen.Select(e => new WohnungListEntry(e)).ToList();
        }
    }
}
