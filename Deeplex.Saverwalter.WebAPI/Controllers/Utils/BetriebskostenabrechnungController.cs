using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class BetriebskostenabrechnungController : ControllerBase
    {
        private readonly ILogger<BetriebskostenabrechnungController> _logger;
        private BetriebskostenabrechnungSerivce Service { get; }

        public BetriebskostenabrechnungController(ILogger<BetriebskostenabrechnungController> logger, BetriebskostenabrechnungSerivce service)
        {
            Service = service;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}")]
        public IActionResult GetBetriebskostenabrechnung(int vertrag_id, int jahr)
        {
            return Service.Get(vertrag_id, jahr, Service.DbService.ctx);
        }
    }
}
