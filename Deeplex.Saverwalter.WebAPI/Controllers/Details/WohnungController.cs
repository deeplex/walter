using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.BetriebskostenrechnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ErhaltungsaufwendungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.UmlageListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ZaehlerListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/wohnungen/{id}")]
    public class WohnungController : ControllerBase
    {
        public class WohnungEntryBase
        {
            protected Wohnung Entity { get; }

            public int id => Entity.WohnungId;
            public string Bezeichnung => Entity.Bezeichnung;
            public double Wohnflaeche => Entity.Wohnflaeche;
            public double Nutzflaeche => Entity.Nutzflaeche;
            public int Einheiten => Entity.Nutzeinheit;
            public string Notiz => Entity.Notiz ?? "";
            public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;
            public string Anschrift => Entity.Adresse.Anschrift + ", " + Entity.Bezeichnung;
            
            public Guid BesitzerId => Entity.BesitzerId;

            public WohnungEntryBase(Wohnung entity)
            {
                Entity = entity;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            private IWalterDbService DbService { get; }

            public IEnumerable<WohnungListEntry> Haus => Entity.Adresse.Wohnungen.Select(e => new WohnungListEntry(e, DbService));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<ZaehlerListEntry> Zaehler => Entity.Zaehler.Select(e => new ZaehlerListEntry(e));
            public IEnumerable<VertragListEntry> Vertraege => Entity.Vertraege.Select(e => new VertragListEntry(e, DbService));
            public IEnumerable<ErhaltungsaufwendungListEntry> Erhaltungsaufwendungen => Entity.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e, DbService));
            public IEnumerable<UmlageListEntry> Umlagen => Entity.Umlagen.Select(e => new UmlageListEntry(e));
            public IEnumerable<BetriebskostenrechungListEntry> Betriebskostenrechnungen => Entity.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechungListEntry(f)));


            public WohnungEntry(Wohnung entity, IWalterDbService dbService) : base(entity)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<WohnungController> _logger;
        private IWalterDbService DbService { get; }

        public WohnungController(ILogger<WohnungController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public WohnungEntry Get(int id)
        {
            return new WohnungEntry(DbService.ctx.Wohnungen.Find(id), DbService);
        }
    }
}