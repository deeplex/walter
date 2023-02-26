using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ControllerService
    {
        protected IWalterDbService DbService { get; }

        public ControllerService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        protected IActionResult Save(BetriebskostenrechnungEntry entry)
        {
            try
            {
                DbService.SaveWalter();
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            };
        }
    }
}
