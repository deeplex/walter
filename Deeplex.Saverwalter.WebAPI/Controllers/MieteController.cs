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
            private Miete? Entity { get; }

            public int? Id { get; set; }
            public double? Betrag { get; set; }
            public DateTime? BetreffenderMonat { get; set; }
            public DateTime? Zahlungsdatum { get; set; }
            public string? Notiz { get; set; }

            public MieteEntryBase() { }
            public MieteEntryBase(Miete entity)
            {
                Entity = entity;
                Id = entity.MieteId;
                Betrag = entity.Betrag;
                BetreffenderMonat = entity.BetreffenderMonat;
                Zahlungsdatum = entity.Zahlungsdatum;
                Notiz = entity.Notiz;
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
