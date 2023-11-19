using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlagetypController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlagetypDbService : IControllerService<UmlagetypEntry>
    {
        public SaverwalterContext Ctx { get; }

        public UmlagetypDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Umlagetypen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new UmlagetypEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Umlagetypen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Umlagetypen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(UmlagetypEntry entry)
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

        private UmlagetypEntry Add(UmlagetypEntry entry)
        {
            var entity = new Umlagetyp(entry.Bezeichnung);
            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Add(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity);
        }

        public IActionResult Put(int id, UmlagetypEntry entry)
        {
            var entity = Ctx.Umlagetypen.Find(id);
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

        private UmlagetypEntry Update(UmlagetypEntry entry, Umlagetyp entity)
        {
            entity.Bezeichnung = entry.Bezeichnung;

            SetOptionalValues(entity, entry);
            Ctx.Umlagetypen.Update(entity);
            Ctx.SaveChanges();

            return new UmlagetypEntry(entity);
        }

        private void SetOptionalValues(Umlagetyp entity, UmlagetypEntry entry)
        {
            entity.Notiz = entry.Notiz;
        }
    }
}
