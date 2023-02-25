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

            public JuristischePersonEntryBase(JuristischePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
                Entity = entity;
            }
        }

        public sealed class JuristischePersonEntry : JuristischePersonEntryBase
        {
            private IWalterDbService DbService { get; }

            public IEnumerable<AnhangListEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<KontaktListEntry> Mitglieder => Entity.Mitglieder.Select(e => new KontaktListEntry(e, DbService));

            public JuristischePersonEntry(JuristischePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<JuristischePersonController> _logger;
        private IWalterDbService DbService { get; }

        public JuristischePersonController(ILogger<JuristischePersonController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public JuristischePersonEntry Get(int id)
        {
            return new JuristischePersonEntry(DbService.ctx.JuristischePersonen.Find(id), DbService);
        }
    }
}
