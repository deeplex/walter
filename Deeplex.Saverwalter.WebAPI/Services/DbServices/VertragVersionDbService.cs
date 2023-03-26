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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private VertragVersionEntryBase Add(VertragVersionEntryBase entry)
        {
            var vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag!.Id!));
            var entity = new VertragVersion(entry.Beginn, entry.Grundmiete, entry.Personenzahl)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            DbService.ctx.VertragVersionen.Add(entity);
            DbService.SaveWalter();

            return new VertragVersionEntryBase(entity);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private VertragVersionEntryBase Update(VertragVersionEntryBase entry, VertragVersion entity)
        {
            entity.Beginn = entry.Beginn;
            entity.Grundmiete = entry.Grundmiete;
            entity.Personenzahl = entry.Personenzahl;

            SetOptionalValues(entity, entry);
            DbService.ctx.VertragVersionen.Update(entity);
            DbService.SaveWalter();

            return new VertragVersionEntryBase(entity);
        }

        private void SetOptionalValues(VertragVersion entity, VertragVersionEntryBase entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
