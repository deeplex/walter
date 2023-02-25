using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

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

            public NatuerlichePersonEntryBase(NatuerlichePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
                Entity = entity;
            }
        }

        public sealed class NatuerlichePersonEntry : NatuerlichePersonEntryBase
        {
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            
            public NatuerlichePersonEntry(NatuerlichePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
            }
        }

        private readonly ILogger<NatuerlichePersonController> _logger;
        private IWalterDbService DbService { get; }

        public NatuerlichePersonController(ILogger<NatuerlichePersonController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "GetNatuerlichePerson")]
        public NatuerlichePersonEntry Get(int id)
        {
            return new NatuerlichePersonEntry(DbService.ctx.NatuerlichePersonen.Find(id), DbService);
        }
    }
}
