using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontakteController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/nat/{id}")]
    public class NatuerlichePersonController : ControllerBase
    {

        public sealed class NatuerlichePersonEntry : PersonEntry
        {
            private NatuerlichePerson Entity { get; }
            public Anrede Anrede => Entity.Anrede;
            public string Vorname => Entity.Vorname ??  "";
            public string Nachname => Entity.Nachname;

            public NatuerlichePersonEntry(NatuerlichePerson entity) : base(entity)
            {
                Entity = entity;
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
