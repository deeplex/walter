﻿using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.VertragListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.WohnungListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class MieterController : ControllerBase
    {
        private readonly ILogger<MieterController> _logger;

        public MieterController(ILogger<MieterController> logger)
        {
            _logger = logger;
        }

        private IQueryable<Vertrag> GetQueryableVertrag(Guid id)
        {
            var Person = Program.ctx.FindPerson(id).JuristischePersonen.Select(e => e.PersonId).ToList();
            var asMieter = Program.ctx.MieterSet.Where(e => e.PersonId == id || Person.Contains(e.PersonId)).Select(e => e.Vertrag).ToList();
            var asOther = Program.ctx.Vertraege.Where(e => 
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
        public IEnumerable<WohnungListEntry> GetWohnungen(Guid id)
        {
            return GetQueryableWohnung(id).Select(e => new WohnungListEntry(e)).ToList();
        }

        [HttpGet]
        [Route("api/vertraege/mieter/{id}")]
        public IEnumerable<VertragListEntry> GetVertraege(Guid id)
        {
            return GetQueryableVertrag(id).Select(e => new VertragListEntry(e)).ToList();
        }
    }
}
