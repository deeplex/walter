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
            public KontaktListEntry(NatuerlichePerson n)
            {
                Entity = n;
                Id = n.NatuerlichePersonId;
            }
            public KontaktListEntry(JuristischePerson j)
            {
                Entity = j;
                Id = - j.JuristischePersonId;
            }
            public KontaktListEntry(IPerson p)
            {
                if (Program.ctx.FindPerson(p.PersonId) is NatuerlichePerson n)
                {
                    Entity = n;
                    Id = n.NatuerlichePersonId;
                }
                else if (Program.ctx.FindPerson(p.PersonId) is JuristischePerson j)
                {
                    Entity = j;
                    Id =  - j.JuristischePersonId;
                }
                else
                {
                    throw new EntryPointNotFoundException();
                }
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
            var np = Program.ctx.NatuerlichePersonen.Select(e => new KontaktListEntry(e)).ToList();
            var jp = Program.ctx.JuristischePersonen.Select(e => new KontaktListEntry(e)).ToList();
            return np.Concat(jp);
        }
    }
}
