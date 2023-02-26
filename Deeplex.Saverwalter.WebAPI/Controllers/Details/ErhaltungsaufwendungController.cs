using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen/{id}")]
    public class ErhaltungsaufwendungController : ControllerBase
    {
        public class ErhaltungsaufwendungEntryBase
        {
            protected Erhaltungsaufwendung? Entity { get; }

            public int Id { get; set; }
            public double Betrag { get; set; }
            public DateTime Datum { get; set; }
            public string? Notiz { get; set; }
            public string? Bezeichnung { get; set; }

            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity)
            {
                Entity = entity;

                Id = Entity.ErhaltungsaufwendungId;
                Betrag = Entity.Betrag;
                Datum = Entity.Datum;
                Notiz = Entity.Notiz;
                Bezeichnung = Entity.Bezeichnung;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            IWalterDbService? DbService { get; }

            public PersonEntryBase? Aussteller => DbService?.ctx.FindPerson(Entity!.AusstellerId) is IPerson p ? new(p) : null;
            public WohnungEntryBase? Wohnung => new(Entity!.Wohnung);
            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));

            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<ErhaltungsaufwendungController> _logger;
        private ErhaltungsaufwendungDbService DbService { get; }

        public ErhaltungsaufwendungController(ILogger<ErhaltungsaufwendungController> logger, ErhaltungsaufwendungDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] ErhaltungsaufwendungEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, [FromBody] ErhaltungsaufwendungEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
