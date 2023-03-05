using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using Deeplex.Saverwalter.WebAPI.Helper;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mieten")]
    public class MieteController : ControllerBase
    {
        public class MieteEntryBase
        {
            private Miete? Entity { get; }

            public int Id { get; set; }
            public double? Betrag { get; set; }
            public DateTime? BetreffenderMonat { get; set; }
            public DateTime? Zahlungsdatum { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry? Vertrag { get; set; }

            public MieteEntryBase() { }
            public MieteEntryBase(Miete entity)
            {
                Entity = entity;
                Id = entity.MieteId;
                Betrag = entity.Betrag;
                BetreffenderMonat = entity.BetreffenderMonat;
                Zahlungsdatum = entity.Zahlungsdatum;
                Notiz = entity.Notiz;
                var v = entity.Vertrag;
                var a = v.Wohnung.Adresse.Anschrift;
                Vertrag = new(v.VertragId, a + " - " + v.Wohnung.Bezeichnung + " - " + Zahlungsdatum.Datum());
            }
        }

        private readonly ILogger<MieteController> _logger;
        private MieteDbService DbService { get; }

        public MieteController(ILogger<MieteController> logger, MieteDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
            => new OkObjectResult(DbService.ctx.Mieten.ToList().Select(e => new MieteEntryBase(e)).ToList());
        [HttpPost]
        public IActionResult Post([FromBody] MieteEntryBase entry) => DbService.Post(entry);

        [HttpGet("{id}")]
        public IActionResult Get(int id) => DbService.Get(id);
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MieteEntryBase entry) => DbService.Put(id, entry);
        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
