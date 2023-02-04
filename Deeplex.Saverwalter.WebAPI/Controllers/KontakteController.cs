using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;

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
            public string Name { get; }
            public string Email => Entity.Email ?? "";
            public string Telefon => Entity.Telefon ?? "";

            public string Anschrift => "TODO";

            public KontaktListEntry(IPerson p)
            {
                Entity = p;
                Name = Entity.Bezeichnung;
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
            var np = Program.ctx.NatuerlichePersonen.Select(e => new KontaktListEntry(e)).ToList();
            var jp = Program.ctx.JuristischePersonen.Select(e => new KontaktListEntry(e)).ToList();
            return np.Concat(jp);
        }
    }
}
