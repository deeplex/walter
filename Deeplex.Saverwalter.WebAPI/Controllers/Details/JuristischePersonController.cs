using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Helper;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.BetriebskostenrechnungController;
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
            protected new JuristischePerson? Entity { get; }
            public int Id { get; set; }

            public JuristischePersonEntryBase() : base() { }
            public JuristischePersonEntryBase(JuristischePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
                Entity = entity;

                Id = Entity.JuristischePersonId;
            }
        }

        public sealed class JuristischePersonEntry : JuristischePersonEntryBase
        {
            private IWalterDbService DbService { get; set; } = null!;

            public IEnumerable<AnhangListEntry>? Anhaenge => Entity?.Anhaenge.Select(e => new AnhangListEntry(e));
            public IEnumerable<KontaktListEntry>? Mitglieder => Entity?.Mitglieder.Select(e => new KontaktListEntry(e, DbService));

            public JuristischePersonEntry() : base() { }
            public JuristischePersonEntry(JuristischePerson entity, IWalterDbService dbService) : base(entity, dbService)
            {
                DbService = dbService;
            }
        }

        private readonly ILogger<JuristischePersonController> _logger;
        private JuristischePersonDbService DbService { get; }

        public JuristischePersonController(ILogger<JuristischePersonController> logger, JuristischePersonDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet(Name = "[Controller]")]
        public IActionResult Get(int id) => DbService.Get(id);

        [HttpPost(Name = "[Controller]")]
        public IActionResult Post([FromBody] JuristischePersonEntry entry) => DbService.Post(entry);

        [HttpPut(Name = "[Controller]")]
        public IActionResult Put(int id, JuristischePersonEntry entry) => DbService.Put(id, entry);

        [HttpDelete(Name = "[Controller]")]
        public IActionResult Delete(int id) => DbService.Delete(id);
    }
}
