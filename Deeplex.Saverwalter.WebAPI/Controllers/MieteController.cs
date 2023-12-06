using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/mieten")]
    public class MieteController : ControllerBase
    {
        public class MieteEntryBase
        {
            public int Id { get; set; }
            public double Betrag { get; set; }
            public DateOnly BetreffenderMonat { get; set; }
            public DateOnly Zahlungsdatum { get; set; }
            public string? Notiz { get; set; }
            public SelectionEntry Vertrag { get; set; } = null!;
            public int Repeat { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public MieteEntryBase() { }
            public MieteEntryBase(Miete entity, int repeat = 0)
            {
                Id = entity.MieteId;
                Betrag = entity.Betrag;
                BetreffenderMonat = entity.BetreffenderMonat;
                Zahlungsdatum = entity.Zahlungsdatum;
                Notiz = entity.Notiz;
                var v = entity.Vertrag;
                var a = v.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Vertrag = new(v.VertragId, a + " - " + v.Wohnung.Bezeichnung + " - " + Zahlungsdatum.Datum());
                Repeat = repeat;

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
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
        public Task<IActionResult> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<IActionResult> Post([FromBody] MieteEntryBase entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<IActionResult> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<IActionResult> Put(int id, [FromBody] MieteEntryBase entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
