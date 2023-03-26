using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : IControllerService<ErhaltungsaufwendungEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public ErhaltungsaufwendungDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ErhaltungsaufwendungEntry Add(ErhaltungsaufwendungEntry entry)
        {
            var ausstellerId = new Guid(entry.Aussteller.Id);
            var wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id));
            var entity = new Erhaltungsaufwendung(entry.Betrag, entry.Bezeichnung, ausstellerId, entry.Datum)
            {
                Wohnung = wohnung!
            };

            SetOptionalValues(entity, entry);
            DbService.ctx.Erhaltungsaufwendungen.Add(entity);
            DbService.SaveWalter();

            return new ErhaltungsaufwendungEntry(entity, DbService);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private ErhaltungsaufwendungEntry Update(ErhaltungsaufwendungEntry entry, Erhaltungsaufwendung entity)
        {
            entity.Betrag = entry.Betrag;
            entity.Wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id))!;
            entity.Bezeichnung = entry.Bezeichnung;
            entity.AusstellerId = new Guid(entry.Aussteller.Id);
            entity.Datum = entry.Datum;

            SetOptionalValues(entity, entry);
            DbService.ctx.Erhaltungsaufwendungen.Update(entity);
            DbService.SaveWalter();

            return new ErhaltungsaufwendungEntry(entity, DbService);
        }

        private void SetOptionalValues(Erhaltungsaufwendung entity, ErhaltungsaufwendungEntry entry)
        {
            if (entity.ErhaltungsaufwendungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
