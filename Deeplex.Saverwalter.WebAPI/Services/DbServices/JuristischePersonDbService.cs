using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.JuristischePersonController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class JuristischePersonDbService : BaseDbService<JuristischePersonEntry>, IControllerService<JuristischePersonEntry>
    {
        private void SetValues(JuristischePerson entity, JuristischePersonEntry entry)
        {
            if (entity.JuristischePersonId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            entity.Notiz = entry.Notiz;
        }

        public JuristischePersonDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new JuristischePersonEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.JuristischePersonen.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(JuristischePersonEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new JuristischePerson();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.JuristischePersonen.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, JuristischePersonEntry entry)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.JuristischePersonen.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
