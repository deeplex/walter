using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : BaseDbService<ErhaltungsaufwendungEntry>, IControllerService<ErhaltungsaufwendungEntry>
    {
        private void SetValues(Erhaltungsaufwendung entity, ErhaltungsaufwendungEntry entry)
        {
            if (entity.ErhaltungsaufwendungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Betrag = entry.Betrag;
            entity.Datum = entry.Datum;
            entity.Notiz = entry.Notiz;
            entity.Bezeichnung = entry.Bezeichnung ?? "";
            entity.AusstellerId = new Guid(entry.Aussteller!.Id!);
            entity.Wohnung = Ref.ctx.Wohnungen.Find(int.Parse(entry.Wohnung!.Id!));
        }

        public ErhaltungsaufwendungDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = Ref.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ErhaltungsaufwendungEntry(entity, Ref);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ref.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ref.ctx.Erhaltungsaufwendungen.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(ErhaltungsaufwendungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Erhaltungsaufwendung();

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Erhaltungsaufwendungen.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, ErhaltungsaufwendungEntry entry)
        {
            var entity = Ref.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Erhaltungsaufwendungen.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
