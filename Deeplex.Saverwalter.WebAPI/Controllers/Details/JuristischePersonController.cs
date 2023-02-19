using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.AnhangListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Lists.KontaktListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/kontakte/jur/{id}")]
    public class JuristischePersonController : ControllerBase
    {
        public class JuristischePersonEntryBase : PersonEntry
        {
            protected new JuristischePerson Entity { get; }

            public JuristischePersonEntryBase(JuristischePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        public sealed class JuristischePersonEntry : JuristischePersonEntryBase
        {
            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<KontaktListEntry> Mitglieder => Entity.Mitglieder.Select(e => new KontaktListEntry(e));

            public JuristischePersonEntry(JuristischePerson entity) : base(entity)
            {
            }
        }

        private readonly ILogger<JuristischePersonController> _logger;

        public JuristischePersonController(ILogger<JuristischePersonController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetJuristischePerson")]
        public JuristischePersonEntry Get(int id)
        {
            return new JuristischePersonEntry(Program.ctx.JuristischePersonen.Find(id));
        }
    }
}
