using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.AnhangController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/zaehler/{id}")]
    public class ZaehlerController : ControllerBase
    {
        public class ZaehlerEntryBase
        {
            protected Zaehler Entity { get; }

            public int Id => Entity.ZaehlerId;
            public string Kennnummer => Entity.Kennnummer;
            public Zaehlertyp Typ => Entity.Typ;
            public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;

            public ZaehlerEntryBase(Zaehler entity)
            {
                Entity = entity;
            }
        }

        public class ZaehlerEntry : ZaehlerEntryBase
        {
            public IEnumerable<ZaehlerEntryBase> Einzelzaehler => Entity.EinzelZaehler.Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<ZaehlerstandListEntry> Staende => Entity.Staende.Select(e => new ZaehlerstandListEntry(e));
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public ZaehlerEntryBase? AllgemeinZaehler => Entity.Allgemeinzaehler is Zaehler z ? new ZaehlerEntryBase(z) : null;

            public ZaehlerEntry(Zaehler entity) : base(entity)
            {
            }
        }

        public class ZaehlerstandListEntry
        {
            private Zaehlerstand Entity { get; }
            public int Id => Entity.ZaehlerstandId;
            public string Stand => Entity.Stand.ToString();
            public string Datum => Entity.Datum.Datum();
            //public ZaehlerEntryBase Zaehler => new ZaehlerEntryBase(Entity.Zaehler);

            public ZaehlerstandListEntry(Zaehlerstand entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<ZaehlerController> _logger;
        private IWalterDbService DbService { get; }

        public ZaehlerController(ILogger<ZaehlerController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "GetZaehler")]
        public ZaehlerEntry Get(int id)
        {
            return new ZaehlerEntry(DbService.ctx.ZaehlerSet.Find(id));
        }
    }
}
