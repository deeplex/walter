using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.BetriebskostenrechnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ErhaltungsaufwendungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.UmlageListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.ZaehlerListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/anhaenge/{id}")]
    public class AnhangController : ControllerBase
    {
        public class AnhangEntryBase
        {
            protected Anhang Entity { get; }

            public Guid Id => Entity.AnhangId;
            public string FileName => Entity.FileName;
            public DateTime CreationTime => Entity.CreationTime;
            //public string Notiz => Entity.Notiz;

            public AnhangEntryBase(Anhang a)
            {
                Entity = a;
            }
        }

        public class AnhangEntry : AnhangEntryBase
        {
            public IEnumerable<BetriebskostenrechungListEntry> Betriebskostenrechnungen => Entity.Betriebskostenrechnungen.Select(e => new BetriebskostenrechungListEntry(e));
            public IEnumerable<ErhaltungsaufwendungListEntry> Erhaltungsaufwendungen => Entity.Erhaltungsaufwendungen.Select(e => new ErhaltungsaufwendungListEntry(e));
            public IEnumerable<KontaktListEntry> NatuerlichePersonen => Entity.NatuerlichePersonen.Select(e => new KontaktListEntry(e, e.NatuerlichePersonId, true));
            public IEnumerable<KontaktListEntry> JuristischePersonen => Entity.JuristischePersonen.Select(e => new KontaktListEntry(e, e.JuristischePersonId, false));
            public IEnumerable<UmlageListEntry> Umlagen => Entity.Umlagen.Select(e => new UmlageListEntry(e));
            public IEnumerable<VertragListEntry> Vertraege => Entity.Vertraege.Select(e => new VertragListEntry(e));
            public IEnumerable<WohnungListEntry> Wohnungen => Entity.Wohnungen.Select(e => new WohnungListEntry(e)).ToList();
            public IEnumerable<ZaehlerListEntry> Zaehler => Entity.Zaehler.Select(e => new ZaehlerListEntry(e));

            public AnhangEntry(Anhang entity) : base(entity)
            {
            }
        }

        private readonly ILogger<AnhangController> _logger;

        public AnhangController(ILogger<AnhangController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetAnhang")]
        public AnhangEntry Get(Guid id)
        {
            return new AnhangEntry(Program.ctx.Anhaenge.Find(id));
        }
    }
}
