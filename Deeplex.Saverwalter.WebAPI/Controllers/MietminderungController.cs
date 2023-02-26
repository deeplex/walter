using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mietminderungen")]
    public class MietminderungController : ControllerBase
    {
        public class MietminderungEntryBase
        {
            private Mietminderung Entity { get; }

            public int Id => Entity.MietminderungId;
            public string Betrag => Entity.Beginn.Datum();
            public string? Ende => Entity.Ende.Datum();
            public string Minderung => Entity.Minderung.Prozent();
            public string? Notiz => Entity.Notiz;

            public MietminderungEntryBase(Mietminderung entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<MietminderungController> _logger;
        private IWalterDbService DbService { get; }

        public MietminderungController(ILogger<MietminderungController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get() =>
            new OkObjectResult(DbService.ctx.Mietminderungen.ToList().Select(e => new MietminderungEntryBase(e)).ToList());
    }
}
