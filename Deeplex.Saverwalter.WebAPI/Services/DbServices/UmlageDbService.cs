using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : BaseDbService<UmlageEntry>, IControllerService<UmlageEntry>
    {
        private void SetValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            entity.Notiz = entry.Notiz;
        }

        public UmlageDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = Ref.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new UmlageEntry(entity, Ref);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ref.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ref.ctx.Umlagen.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(UmlageEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Umlage();

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Umlagen.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, UmlageEntry entry)
        {
            var entity = Ref.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Umlagen.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
