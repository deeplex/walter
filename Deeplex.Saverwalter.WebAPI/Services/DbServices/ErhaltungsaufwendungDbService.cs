using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : IControllerService<ErhaltungsaufwendungEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public ErhaltungsaufwendungDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

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
            entity.Wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung!.Id!));
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ErhaltungsaufwendungEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Erhaltungsaufwendungen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
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
                DbService.ctx.Erhaltungsaufwendungen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ErhaltungsaufwendungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, ErhaltungsaufwendungEntry entry)
        {
            var entity = DbService.ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Erhaltungsaufwendungen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new ErhaltungsaufwendungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
