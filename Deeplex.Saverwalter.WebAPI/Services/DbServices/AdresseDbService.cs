using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AdresseDbService : IControllerService<AdresseEntry>
    {
        public SaverwalterContext Ctx { get; }
        public SaverwalterContext ctx => ctx;

        public AdresseDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = ctx.Adressen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new AdresseEntry(entity, ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = ctx.Adressen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            ctx.Adressen.Remove(entity);
            ctx.SaveChanges();

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
            Ctx.Adressen.Add(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity, Ctx);
        }

        public IActionResult Put(int id, AdresseEntry entry)
        {
            var entity = ctx.Adressen.Find(id);
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
            Ctx.Adressen.Update(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity, Ctx);
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
