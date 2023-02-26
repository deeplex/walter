using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public abstract class BaseDbService<T>
    {
        public IWalterDbService Ref { get; }
        public SaverwalterContext ctx => Ref.ctx;

        public BaseDbService(IWalterDbService dbService)
        {
            Ref = dbService;
        }

        protected IActionResult Save(T entry)
        {
            try
            {
                Ref.SaveWalter();
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            };
        }
    }
}
