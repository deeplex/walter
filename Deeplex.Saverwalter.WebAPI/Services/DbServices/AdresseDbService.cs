using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AdresseDbService : IControllerService<AdresseEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public AdresseDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private AdresseEntry Add(AdresseEntry entry)
        {
            var entity = new Adresse(entry.Strasse, entry.Hausnummer, entry.Postleitzahl, entry.Stadt);
            SetOptionalValues(entity, entry);
            DbService.ctx.Adressen.Add(entity);
            DbService.SaveWalter();

            return new AdresseEntry(entity, DbService);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private AdresseEntry Update(AdresseEntry entry, Adresse entity)
        {
            entity.Strasse = entry.Strasse;
            entity.Hausnummer = entry.Hausnummer;
            entity.Postleitzahl = entry.Postleitzahl;
            entity.Stadt = entry.Stadt;

            SetOptionalValues(entity, entry);
            DbService.ctx.Adressen.Update(entity);
            DbService.SaveWalter();

            return new AdresseEntry(entity, DbService);
        }

        private void SetOptionalValues(Adresse entity, AdresseEntry entry)
        {
            if (entity.AdresseId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
