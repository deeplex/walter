using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/anhaenge")]
    public class AnhangListController : ControllerBase
    {
        public class AnahngListEntry
        {
            private Anhang Entity { get; }

            public Guid Id => Entity.AnhangId;
            public string FileName => Entity.FileName;
            public string CreationTime => Entity.CreationTime.Zeit();

            public AnahngListEntry(Anhang a)
            {
                Entity = a;
            }
        }

        private readonly ILogger<AnhangListController> _logger;

        public AnhangListController(ILogger<AnhangListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetAnhaenge")]
        public IEnumerable<AnahngListEntry> Get()
        {
            return Program.ctx.Anhaenge.Select(e => new AnahngListEntry(e)).ToList();
        }
    }
}
