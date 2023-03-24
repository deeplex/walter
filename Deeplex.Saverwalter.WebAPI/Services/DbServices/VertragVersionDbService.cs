using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragVersionDbService : IControllerService<VertragVersionEntryBase>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public VertragVersionDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
        }

        private void SetValues(VertragVersion entity, VertragVersionEntryBase entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Grundmiete = entry.Grundmiete;
            entity.Personenzahl = entry.Personenzahl;
            entity.Beginn = (DateTime)entry.Beginn!;
            entity.Notiz = entry.Notiz;
            entity.Vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag!.Id!));
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.VertragVersionen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new VertragVersionEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.VertragVersionen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.VertragVersionen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(VertragVersionEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new VertragVersion();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.VertragVersionen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragVersionEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, VertragVersionEntryBase entry)
        {
            var entity = DbService.ctx.VertragVersionen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.VertragVersionen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragVersionEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
