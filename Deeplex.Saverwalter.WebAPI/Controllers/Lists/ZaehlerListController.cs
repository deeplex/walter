using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/zaehler")]
    public class ZaehlerListController : ControllerBase
    {
        public class ZaehlerListEntry
        {
            private Zaehler Entity { get; }

            public int id => Entity.ZaehlerId;
            public string Kennnummer => Entity.Kennnummer;
            public Zaehlertyp Typ => Entity.Typ;
            public string Wohnung => Entity.Wohnung is Wohnung w ? w.Adresse.Anschrift + ", " + w.Bezeichnung : "Keine";

            public ZaehlerListEntry(Zaehler z)
            {
                Entity = z;
            }
        }

        private readonly ILogger<ZaehlerListController> _logger;
        private IWalterDbService DbService { get; }

        public ZaehlerListController(ILogger<ZaehlerListController> logger, IWalterDbService dbService)
        {
            _logger = logger;
            DbService = dbService;
        }

        [HttpGet(Name = "GetZaehlerList")]
        public IEnumerable<ZaehlerListEntry> Get()
        {
            return DbService.ctx.ZaehlerSet.Select(e => new ZaehlerListEntry(e)).ToList();
        }
    }
}
