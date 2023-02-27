using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.NatuerlichePersonController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class NatuerlichePersonDbService : BaseDbService<NatuerlichePersonEntry>, IControllerService<NatuerlichePersonEntry>
    {
        private void SetValues(NatuerlichePerson entity, NatuerlichePersonEntry entry)
        {
            if (entity.NatuerlichePersonId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            entity.Notiz = entry.Notiz;
        }

        public NatuerlichePersonDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = Ref.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new NatuerlichePersonEntry(entity, Ref);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ref.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ref.ctx.NatuerlichePersonen.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(NatuerlichePersonEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new NatuerlichePerson();

            try
            {
                SetValues(entity, entry);
                Ref.ctx.NatuerlichePersonen.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, NatuerlichePersonEntry entry)
        {
            var entity = Ref.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                Ref.ctx.NatuerlichePersonen.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
