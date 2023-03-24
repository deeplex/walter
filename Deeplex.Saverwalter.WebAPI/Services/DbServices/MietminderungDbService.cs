using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MietminderungDbService : IControllerService<MietminderungEntryBase>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public MietminderungDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Mietminderung entity, MietminderungEntryBase entry)
        {
            if (entity.MietminderungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beginn = (DateTime)entry.Beginn!;
            entity.Ende = entry.Ende;
            entity.Minderung = (double)entry.Minderung!;
            entity.Notiz = entry.Notiz;
            entity.Vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag!.Id!));
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Mietminderungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new MietminderungEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Mietminderungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Mietminderungen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(MietminderungEntryBase entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Mietminderung();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Mietminderungen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new MietminderungEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, MietminderungEntryBase entry)
        {
            var entity = DbService.ctx.Mietminderungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Mietminderungen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new MietminderungEntryBase(entity));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
