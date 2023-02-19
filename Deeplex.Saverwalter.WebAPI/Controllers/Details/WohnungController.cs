using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
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
            public IEnumerable<WohnungListEntry> Haus => Entity.Adresse.Wohnungen.Select(e => new WohnungListEntry(e));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<ZaehlerListEntry> Zaehler => Entity.Zaehler.Select(e => new ZaehlerListEntry(e));
            public IEnumerable<VertragListEntry> Vertraege => Entity.Vertraege.Select(e => new VertragListEntry(e));
            public IEnumerable<ErhaltungsaufwendungListEntry> Erhaltungsaufwendungen => Entity.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e));
            public IEnumerable<UmlageListEntry> Umlagen => Entity.Umlagen.Select(e => new UmlageListEntry(e));
            public IEnumerable<BetriebskostenrechungListEntry> Betriebskostenrechnungen => Entity.Umlagen.SelectMany(e => e.Betriebskostenrechnungen.Select(f => new BetriebskostenrechungListEntry(f)));


            public WohnungEntry(Wohnung entity) : base(entity) { }
        }

        private readonly ILogger<WohnungController> _logger;

        public WohnungController(ILogger<WohnungController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWohnung")]
        public WohnungEntry Get(int id)
        {
            return new WohnungEntry(Program.ctx.Wohnungen.Find(id));
        }
    }
}