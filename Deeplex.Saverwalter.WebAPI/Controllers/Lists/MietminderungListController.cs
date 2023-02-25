using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/mietminderungen")]
    public class MietminderungListController : ControllerBase
    {
        public class MietminderungListEntry
        {
            private Mietminderung Entity { get; }

            public int Id => Entity.MietminderungId;
            public string Betrag => Entity.Beginn.Datum();
            public string? Ende => Entity.Ende.Datum();
            public string Minderung => Entity.Minderung.Prozent();
            public string? Notiz => Entity.Notiz;

            public MietminderungListEntry(Mietminderung entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<MietminderungListController> _logger;
        private IWalterDbService DbService { get; }

        public MietminderungListController(ILogger<MietminderungListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IEnumerable<MietminderungListEntry> Get()
        {
            return DbService.ctx.Mietminderungen.Select(e => new MietminderungListEntry(e)).ToList();
        }
    }
}
