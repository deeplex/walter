using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class MieterController : ControllerBase
    {
        private readonly ILogger<MieterController> _logger;
        private SaverwalterContext Ctx { get; }

        public MieterController(ILogger<MieterController> logger, SaverwalterContext ctx)
        {
            Ctx = ctx;
            _logger = logger;
        }

        private IQueryable<Vertrag> GetQueryableVertrag(Guid id)
        {
            var Person = Ctx.FindPerson(id).JuristischePersonen.Select(e => e.PersonId).ToList();
            var asMieter = Ctx.MieterSet.Where(e => e.PersonId == id || Person.Contains(e.PersonId)).Select(e => e.Vertrag).ToList();
            var asOther = Ctx.Vertraege.Where(e =>
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
            return GetQueryableWohnung(id).Select(e => new WohnungEntryBase(e, Ctx)).ToList();
        }

        [HttpGet]
        [Route("api/vertraege/mieter/{id}")]
        public IEnumerable<VertragEntryBase> GetVertraege(Guid id)
        {
            return GetQueryableVertrag(id).Select(e => new VertragEntryBase(e, Ctx)).ToList();
        }
    }
}
