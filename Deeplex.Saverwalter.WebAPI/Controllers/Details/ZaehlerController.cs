using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/zaehler/{id}")]
    public class ZaehlerController
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
            public IEnumerable<ZaehlerEntryBase> EinzelZaehler => Entity.EinzelZaehler.Select(e => new ZaehlerEntryBase(e));
            public IEnumerable<ZaehlerStandEntry> Staende => Entity.Staende.Select(e => new ZaehlerStandEntry(e));
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));
            public ZaehlerEntryBase? AllgemeinZaehler => Entity.Allgemeinzaehler is Zaehler z ? new ZaehlerEntryBase(z) : null;

            public ZaehlerEntry(Zaehler entity) : base(entity)
            {
            }
        }

        public class ZaehlerStandEntry
        {
            private Zaehlerstand Entity { get; }
            public int Id => Entity.ZaehlerstandId;
            public double Stand => Entity.Stand;
            public DateTime Datum => Entity.Datum;
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));
            public ZaehlerEntryBase Zaehler => new ZaehlerEntryBase(Entity.Zaehler);

            public ZaehlerStandEntry(Zaehlerstand entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<ZaehlerController> _logger;

        public ZaehlerController(ILogger<ZaehlerController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetZaehler")]
        public ZaehlerEntry Get(int id)
        {
            return new ZaehlerEntry(Program.LoadNavigations(Program.ctx.ZaehlerSet.Find(id)));
        }
    }
}
