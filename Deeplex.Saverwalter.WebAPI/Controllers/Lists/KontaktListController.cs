using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/kontakte")]
    public class KontaktListController : ControllerBase
    {
        public class KontaktListEntry
        {
            private IPerson Entity { get; }

            public Guid Guid => Entity.PersonId;
            public int Id { get; }
            public string Name => Entity.Bezeichnung;
            public string Email => Entity.Email ?? "";
            public string Telefon => Entity.Telefon ?? "";
            public string Mobil => Entity.Mobil ?? "";

            public string Anschrift => Entity.Adresse is Adresse a ? a.Anschrift : "Unbekannt";

            public KontaktListEntry(IPerson p, int id, bool n)
            {
                Entity = p;
                Id = id * (n ? 1 : -1);
            }
        }

        private readonly ILogger<KontaktListController> _logger;

        public KontaktListController(ILogger<KontaktListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetKontaktList")]
        public IEnumerable<KontaktListEntry> Get()
        {
            var np = Program.ctx.NatuerlichePersonen.Select(e => new KontaktListEntry(e, e.NatuerlichePersonId, true)).ToList();
            var jp = Program.ctx.JuristischePersonen.Select(e => new KontaktListEntry(e, e.JuristischePersonId, false)).ToList();
            return np.Concat(jp);
        }
    }
}
