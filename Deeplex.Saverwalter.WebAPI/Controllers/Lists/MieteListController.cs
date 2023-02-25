using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/mieten")]
    public class MieteListController : ControllerBase
    {
        public class MieteListEntry
        {
            private Miete Entity { get; }

            public int Id => Entity.MieteId;
            public string Betrag => Entity.Betrag.Euro();
            public string BetreffenderMonat => Entity.BetreffenderMonat.Datum();
            public string Zahlungsdatum => Entity.Zahlungsdatum.Datum();
            public string? Notiz => Entity.Notiz;

            public MieteListEntry(Miete entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<MieteListController> _logger;
        private IWalterDbService DbService { get; }

        public MieteListController(ILogger<MieteListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IEnumerable<MieteListEntry> Get()
        {
            return DbService.ctx.Mieten.Select(e => new MieteListEntry(e)).ToList();
        }
    }
}
