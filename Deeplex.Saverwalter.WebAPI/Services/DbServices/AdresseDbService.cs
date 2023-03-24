using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AdresseDbService : IControllerService<AdresseEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public AdresseDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Adresse entity, AdresseEntry entry)
        {
            if (entity.AdresseId != entry.Id)
            {
                throw new Exception();
            }

            entity.Strasse = entry.Strasse!;
            entity.Hausnummer = entry.Hausnummer!;
            entity.Postleitzahl = entry.Postleitzahl!;
            entity.Stadt = entry.Stadt!;
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Adressen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new AdresseEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Adressen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Adressen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(AdresseEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Adresse();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Adressen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new AdresseEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, AdresseEntry entry)
        {
            var entity = DbService.ctx.Adressen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Adressen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new AdresseEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
