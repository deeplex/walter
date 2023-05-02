using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerstandDbService : IControllerService<ZaehlerstandEntryBase>
    {
        public SaverwalterContext Ctx { get; }

        public ZaehlerstandDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Zaehlerstaende.Find(id);
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
            var entity = Ctx.Zaehlerstaende.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Zaehlerstaende.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(ZaehlerstandEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ZaehlerstandEntryBase Add(ZaehlerstandEntryBase entry)
        {
            var zaehler = Ctx.ZaehlerSet.Find(int.Parse(entry.Zaehler!.Id!));
            var entity = new Zaehlerstand(entry.Datum, entry.Stand)
            {
                Zaehler = zaehler!
            };
            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntryBase(entity);
        }

        public IActionResult Put(int id, ZaehlerstandEntryBase entry)
        {
            var entity = Ctx.Zaehlerstaende.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ZaehlerstandEntryBase Update(ZaehlerstandEntryBase entry, Zaehlerstand entity)
        {
            entity.Datum = entry.Datum;
            entity.Stand = entry.Stand;

            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Update(entity);
            Ctx.SaveChanges();

            return new ZaehlerstandEntryBase(entity);
        }

        private void SetOptionalValues(Zaehlerstand entity, ZaehlerstandEntryBase entry)
        {
            entity.Stand = entry.Stand;
            entity.Notiz = entry.Notiz;
        }
    }
}
