using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/[controller]")]
    public class KontakteController : ControllerBase
    {
        public class KontaktListEntry
        {
            private IPerson Entity { get; }

            public Guid Guid => Entity.PersonId;
            public int Id { get; }
            public bool Natuerlich { get; }
            public string Name => Entity.Bezeichnung;
            public string Email => Entity.Email ?? "";
            public string Telefon => Entity.Telefon ?? "";
            public string Mobil => Entity.Mobil ?? "";

            public string Anschrift => Entity.Adresse is Adresse a ? a.Anschrift : "Unbekannt";

            public KontaktListEntry(IPerson p, int id, bool n)
            {
                Entity = p;
                Id = id;
                Natuerlich = n;
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
            var np = Program.ctx.NatuerlichePersonen.Include(e => e.Adresse).ToList().Select(e => new KontaktListEntry(e, e.NatuerlichePersonId, true)).ToList();
            var jp = Program.ctx.JuristischePersonen.Include(e => e.Adresse).ToList().Select(e => new KontaktListEntry(e, e.JuristischePersonId, false)).ToList();
            return np.Concat(jp);
        }
    }
}
