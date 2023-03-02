using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AnhangController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AnhangDbService //, IControllerService<AnhangEntry> // TODO because Guid => Change Guid with int
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public AnhangDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Anhang entity, AnhangEntry entry)
        {
            if (entity.AnhangId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            //entity.Notiz = entry.Notiz;
        }

        public IActionResult Get(Guid id)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new AnhangEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(Guid id)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Anhaenge.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(AnhangEntry entry)
        {
            if (entry.Id != Guid.Empty)
            {
                return new BadRequestResult();
            }
            var entity = new Anhang();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Anhaenge.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new AnhangEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(Guid id, AnhangEntry entry)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Anhaenge.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new AnhangEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
