using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerstandDbService : IControllerService<ZaehlerstandEntryBase>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public ZaehlerstandDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Zaehlerstand entity, ZaehlerstandEntryBase entry)
        {
            if (entity.ZaehlerstandId != entry.Id)
            {
                throw new Exception();
            }

            entity.Stand = entry.Stand;
            entity.Datum = (DateTime)entry.Datum!;
            entity.Notiz = entry.Notiz;
            entity.Zaehler = ctx.ZaehlerSet.Find(int.Parse(entry.Zaehler!.Id!));
        }


        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Zaehlerstaende.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ZaehlerstandEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Zaehlerstaende.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Zaehlerstaende.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(ZaehlerstandEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Zaehlerstand();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Zaehlerstaende.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ZaehlerstandEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, ZaehlerstandEntryBase entry)
        {
            var entity = DbService.ctx.Zaehlerstaende.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Zaehlerstaende.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ZaehlerstandEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
