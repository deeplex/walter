using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mieten")]
    public class MieteController : ControllerBase
    {
        public class MieteEntryBase
        {
            private Miete Entity { get; }

            public int Id => Entity.MieteId;
            public string Betrag => Entity.Betrag.Euro();
            public string BetreffenderMonat => Entity.BetreffenderMonat.Datum();
            public string Zahlungsdatum => Entity.Zahlungsdatum.Datum();
            public string? Notiz => Entity.Notiz;

            public MieteEntryBase(Miete entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<MieteController> _logger;
        private IWalterDbService DbService { get; }

        public MieteController(ILogger<MieteController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.ctx.Mieten.ToList().Select(e => new MieteEntryBase(e)).ToList());
    }
}
