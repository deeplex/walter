using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

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

            public SelectionEntry Vertrag { get; set; } = null!;

            public Permissions Permissions { get; set; } = new Permissions();

            public MieteEntryBase() { }
            public MieteEntryBase(Miete entity, Permissions permissions)
            {
                Id = entity.MieteId;
                Betrag = entity.Betrag;
                BetreffenderMonat = entity.BetreffenderMonat;
                Zahlungsdatum = entity.Zahlungsdatum;

                var v = entity.Vertrag;
                var a = v.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Vertrag = new(v.VertragId, a + " - " + v.Wohnung.Bezeichnung + " - " + Zahlungsdatum.Datum());

                Permissions = permissions;
            }
        }

        public class MieteEntry : MieteEntryBase
        {
            public string? Notiz { get; set; }
            public int Repeat { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }

            public MieteEntry() { }
            public MieteEntry(Miete entity, Permissions permissions, int repeat = 0) : base(entity, permissions)
            {
                Repeat = repeat;
                Notiz = entity.Notiz;

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
        public Task<ActionResult<IEnumerable<MieteEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<MieteEntry>> Post([FromBody] MieteEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<MieteEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<MieteEntry>> Put(int id, [FromBody] MieteEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
