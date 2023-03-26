using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MieteDbService : IControllerService<MieteEntryBase>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public MieteDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new MieteEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Mieten.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(MieteEntryBase entry)
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

        private MieteEntryBase Add(MieteEntryBase entry)
        {

            var vertrag = ctx.Vertraege.Find(int.Parse(entry.Vertrag.Id));
            var entity = new Miete(entry.Zahlungsdatum, entry.BetreffenderMonat, entry.Betrag)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            DbService.ctx.Mieten.Add(entity);
            DbService.SaveWalter();

            return new MieteEntryBase(entity);
        }


        public IActionResult Put(int id, MieteEntryBase entry)
        {
            var entity = DbService.ctx.Mieten.Find(id);
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

        private MieteEntryBase Update(MieteEntryBase entry, Miete entity)
        {
            entity.BetreffenderMonat = entry.BetreffenderMonat;
            entity.Betrag = entry.Betrag;
            entity.Zahlungsdatum = entry.Zahlungsdatum;

            SetOptionalValues(entity, entry);
            DbService.ctx.Mieten.Update(entity);
            DbService.SaveWalter();

            return new MieteEntryBase(entity);
        }

        private void SetOptionalValues(Miete entity, MieteEntryBase entry)
        {
            if (entity.MieteId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
