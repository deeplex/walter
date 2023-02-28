using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : BaseDbService<VertragEntry>, IControllerService<VertragEntry>
    {
        private void SetValues(Vertrag entity, VertragEntry entry)
        {
            if (entity.VertragId != entry.Id)
            {
                throw new Exception();
            }

            // TODO Vertragsversionen
            // Beginn => Versionen
            entity.Ende = entry.Ende;
            entity.Wohnung = Ref.ctx.Wohnungen.Find(int.Parse(entry.Wohnung!.Id!));
            entity.AnsprechpartnerId = entry.Ansprechpartner is SelectionEntry s ? new Guid(s.Id!) : null;
            entity.Notiz = entry.Notiz;
        }

        public VertragDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = Ref.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new VertragEntry(entity, Ref);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ref.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ref.ctx.Vertraege.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(VertragEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Vertrag();

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Vertraege.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, VertragEntry entry)
        {
            var entity = Ref.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Vertraege.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
