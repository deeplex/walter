using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontakteController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/{guid}")]
    public class PersonController : ControllerBase
    {
        public sealed class NatuerlichePersonEntry : PersonEntry
        {
            private NatuerlichePerson Entity { get; }

            public string Vorname => Entity.Vorname ?? "";
            public string Nachname => Entity.Nachname;
            public string Anrede => Entity.Anrede.ToString();

            public NatuerlichePersonEntry(NatuerlichePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        public sealed class JuristischePersonEntry : PersonEntry
        {
            private JuristischePerson Entity { get; }

            public JuristischePersonEntry(JuristischePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        public class PersonEntry
        {
            private IPerson Entity { get; }

            public Guid Guid => Entity.PersonId;
            public string Name => Entity.Bezeichnung;
            public string Email => Entity.Email ?? "";
            public string Fax => Entity.Fax ?? "";
            public string Notiz => Entity.Notiz ?? "";
            public bool natuerlichePerson => Entity is NatuerlichePerson;
            //public IEnumerable<KontaktListEntry> JuristischePersonen => Entity.JuristischePersonen.Select(e => new KontaktListEntry(e));
            public string Telefon => Entity.Telefon ?? "";
            public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

            public PersonEntry(IPerson entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<PersonController> _logger;

        public PersonController(ILogger<PersonController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetPerson")]
        public PersonEntry Get(Guid guid)
        {
            var person = Program.FindPerson(guid);
            if (person is NatuerlichePerson n)
            {
                return new NatuerlichePersonEntry(n);
            }
            else if (person is JuristischePerson j)
            {
                return new JuristischePersonEntry(j);
            }
            return null!;
        }
    }
}
