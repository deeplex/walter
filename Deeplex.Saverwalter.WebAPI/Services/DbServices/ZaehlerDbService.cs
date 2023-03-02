using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerDbService : IControllerService<ZaehlerEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public ZaehlerDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Zaehler entity, ZaehlerEntry entry)
        {
            if (entity.ZaehlerId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            entity.Notiz = entry.Notiz;
        }


        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.ZaehlerSet.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ZaehlerEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.ZaehlerSet.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.ZaehlerSet.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(ZaehlerEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Zaehler();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.ZaehlerSet.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ZaehlerEntry(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, ZaehlerEntry entry)
        {
            var entity = DbService.ctx.ZaehlerSet.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.ZaehlerSet.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ZaehlerEntry(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
