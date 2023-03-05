using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MieteDbService : IControllerService<MieteEntryBase>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public MieteDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Miete entity, MieteEntryBase entry)
        {
            if (entity.MieteId != entry.Id)
            {
                throw new Exception();
            }

            entity.BetreffenderMonat = (DateTime)entry.BetreffenderMonat!;
            entity.Betrag = entry.Betrag;
            entity.Notiz = entry.Notiz;
            entity.Zahlungsdatum = (DateTime)entry.Zahlungsdatum!;
            entity.Vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag!.Id!));
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new MieteEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Mieten.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(MieteEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Miete();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Mieten.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new MieteEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, MieteEntryBase entry)
        {
            var entity = DbService.ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Mieten.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new MieteEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
