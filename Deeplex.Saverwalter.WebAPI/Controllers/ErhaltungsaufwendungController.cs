using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Controllers.Utils;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen")]
    public class ErhaltungsaufwendungController : FileControllerBase<ErhaltungsaufwendungEntry, Erhaltungsaufwendung>
    {
        public class ErhaltungsaufwendungEntryBase
        {
            protected Erhaltungsaufwendung? Entity { get; }

            public int Id { get; set; }
            public double Betrag { get; set; }
            public DateOnly Datum { get; set; }
            public string Bezeichnung { get; set; } = null!;
            public SelectionEntry Aussteller { get; set; } = null!;

            public Permissions Permissions { get; set; } = new Permissions();

            public ErhaltungsaufwendungEntryBase() { }
            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity, Permissions permissions)
            {
                Entity = entity;

                Id = Entity.ErhaltungsaufwendungId;
                Betrag = Entity.Betrag;
                Datum = Entity.Datum;
                Bezeichnung = Entity.Bezeichnung;
                Aussteller = new(Entity.Aussteller.KontaktId, Entity.Aussteller.Bezeichnung);


                Permissions = permissions;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            public SelectionEntry Wohnung { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public string? Notiz { get; set; }

            public ErhaltungsaufwendungEntry() { }
            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity, Permissions permissions) : base(entity, permissions)
            {
                Notiz = entity.Notiz;
                var anschrift = entity.Wohnung.Adresse is Adresse a ? a.Anschrift : "Unbekannte Anschrift";
                Wohnung = new(entity.Wohnung.WohnungId, $"{anschrift} - {entity.Wohnung.Bezeichnung}");

                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
            }
        }

        private readonly ILogger<ErhaltungsaufwendungController> _logger;
        protected override ErhaltungsaufwendungDbService DbService { get; }

        public ErhaltungsaufwendungController(ILogger<ErhaltungsaufwendungController> logger, ErhaltungsaufwendungDbService dbService, HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<ErhaltungsaufwendungEntryBase>>> Get() => DbService.GetList(User!);

        [HttpPost]
        public Task<ActionResult<ErhaltungsaufwendungEntry>> Post([FromBody] ErhaltungsaufwendungEntry entry) => DbService.Post(User!, entry);

        [HttpGet("{id}")]
        public Task<ActionResult<ErhaltungsaufwendungEntry>> Get(int id) => DbService.Get(User!, id);
        [HttpPut("{id}")]
        public Task<ActionResult<ErhaltungsaufwendungEntry>> Put(int id, [FromBody] ErhaltungsaufwendungEntry entry) => DbService.Put(User!, id, entry);
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(int id) => DbService.Delete(User!, id);
    }
}
