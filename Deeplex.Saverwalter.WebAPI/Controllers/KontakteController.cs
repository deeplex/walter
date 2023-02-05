using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KontakteController : ControllerBase
    {
        public class KontaktListEntry
        {
            private IPerson Entity { get; }

            public Guid Guid => Entity.PersonId;
            public string Name => Entity.Bezeichnung;
            public string Email => Entity.Email ?? "";
            public string Telefon => Entity.Telefon ?? "";

            public string Anschrift => Entity.Adresse is Adresse a ? a.Anschrift : "Unbekannt";

            public KontaktListEntry(IPerson p)
            {
                Entity = p;
            }
        }

        private readonly ILogger<KontakteController> _logger;

        public KontakteController(ILogger<KontakteController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetKontakte")]
        public IEnumerable<KontaktListEntry> Get()
        {
            var np = Program.ctx.NatuerlichePersonen.Include(e => e.Adresse).ToList().Select(e => new KontaktListEntry(e)).ToList();
            var jp = Program.ctx.JuristischePersonen.Include(e => e.Adresse).ToList().Select(e => new KontaktListEntry(e)).ToList();
            return np.Concat(jp);
        }
    }
}
