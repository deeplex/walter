using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class MieterController : ControllerBase
    {
        private readonly ILogger<MieterController> _logger;
        private WalterDbService DbService { get; }

        public MieterController(ILogger<MieterController> logger, WalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        private IQueryable<Vertrag> GetQueryableVertrag(Guid id)
        {
            var Person = DbService.ctx.FindPerson(id).JuristischePersonen.Select(e => e.PersonId).ToList();
            var asMieter = DbService.ctx.MieterSet.Where(e => e.PersonId == id || Person.Contains(e.PersonId)).Select(e => e.Vertrag).ToList();
            var asOther = DbService.ctx.Vertraege.Where(e =>
                id == e.Wohnung.BesitzerId ||
                id == e.AnsprechpartnerId ||
                Person.Contains(e.Wohnung.BesitzerId) ||
                (e.AnsprechpartnerId.HasValue ? Person.Contains(e.AnsprechpartnerId ?? Guid.Empty) : false)).ToList();

            asOther.AddRange(asMieter);

            return asOther.AsQueryable().DistinctBy(e => e.VertragId);
        }

        private IQueryable<Wohnung> GetQueryableWohnung(Guid id)
        {
            return GetQueryableVertrag(id).Select(e => e.Wohnung).Distinct();
        }

        [HttpGet]
        [Route("api/wohnungen/mieter/{id}")]
        public IEnumerable<WohnungEntryBase> GetWohnungen(Guid id)
        {
            return GetQueryableWohnung(id).Select(e => new WohnungEntryBase(e, DbService)).ToList();
        }

        [HttpGet]
        [Route("api/vertraege/mieter/{id}")]
        public IEnumerable<VertragEntryBase> GetVertraege(Guid id)
        {
            return GetQueryableVertrag(id).Select(e => new VertragEntryBase(e, DbService)).ToList();
        }
    }
}
