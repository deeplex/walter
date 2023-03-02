using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : IControllerService<VertragEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public VertragDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Vertrag entity, VertragEntry entry)
        {
            if (entity.VertragId != entry.Id)
            {
                throw new Exception();
            }

            // TODO Vertragsversionen
            // Beginn => Versionen
            entity.Ende = entry.Ende;
            entity.Wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung!.Id!));
            entity.AnsprechpartnerId = entry.Ansprechpartner is SelectionEntry s ? new Guid(s.Id!) : null;
            entity.Notiz = entry.Notiz;
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new VertragEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Vertraege.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
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
                DbService.ctx.Vertraege.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, VertragEntry entry)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Vertraege.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
