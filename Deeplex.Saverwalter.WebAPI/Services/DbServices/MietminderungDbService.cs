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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private MietminderungEntryBase Add(MietminderungEntryBase entry)
        {
            var vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag.Id));
            var entity = new Mietminderung(entry.Beginn, entry.Minderung)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            DbService.ctx.Mietminderungen.Add(entity);
            DbService.SaveWalter();

            return new MietminderungEntryBase(entity);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private MietminderungEntryBase Update(MietminderungEntryBase entry, Mietminderung entity)
        {
            entity.Ende = entry.Ende;
            entity.Beginn = entry.Beginn;
            entity.Minderung = entry.Minderung;

            SetOptionalValues(entity, entry);
            DbService.ctx.Mietminderungen.Update(entity);
            DbService.SaveWalter();

            return new MietminderungEntryBase(entity);
        }

        private void SetOptionalValues(Mietminderung entity, MietminderungEntryBase entry)
        {
            if (entity.MietminderungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
