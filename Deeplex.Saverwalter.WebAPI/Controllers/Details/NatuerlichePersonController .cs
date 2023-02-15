using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/kontakte/nat/{id}")]
    public class NatuerlichePersonController : ControllerBase
    {
        public class NatuerlichePersonEntryBase : PersonEntry
        {
            protected new NatuerlichePerson Entity { get; }

            public Anrede Anrede => Entity.Anrede;
            public string Vorname => Entity.Vorname ?? "";
            public string Nachname => Entity.Nachname;

            public NatuerlichePersonEntryBase(NatuerlichePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        public sealed class NatuerlichePersonEntry : NatuerlichePersonEntryBase
        {
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));
            
            public NatuerlichePersonEntry(NatuerlichePerson entity) : base(entity)
            {
            }
        }

        private readonly ILogger<NatuerlichePersonController> _logger;

        public NatuerlichePersonController(ILogger<NatuerlichePersonController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetNatuerlichePerson")]
        public NatuerlichePersonEntry Get(int id)
        {
            return new NatuerlichePersonEntry(Program.ctx.NatuerlichePersonen.Find(id));
        }
    }
}
