using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class WohnungDbService : IControllerService<WohnungEntry>
    {
        public SaverwalterContext Ctx { get; }

        public WohnungDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Wohnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new WohnungEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Wohnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Wohnungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(WohnungEntry entry)
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

        private WohnungEntry Add(WohnungEntry entry)
        {
            var entity = new Wohnung(entry.Bezeichnung, entry.Wohnflaeche, entry.Nutzflaeche, entry.Einheiten)
            {
                Besitzer = Ctx.Kontakte.Find(entry.Besitzer!.Id)!
            };

            SetOptionalValues(entity, entry);
            Ctx.Wohnungen.Add(entity);
            Ctx.SaveChanges();

            return new WohnungEntry(entity);
        }

        public IActionResult Put(int id, WohnungEntry entry)
        {
            var entity = Ctx.Wohnungen.Find(id);
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

        private WohnungEntry Update(WohnungEntry entry, Wohnung entity)
        {
            entity.Bezeichnung = entry.Bezeichnung;
            entity.Wohnflaeche = entry.Wohnflaeche;
            entity.Nutzflaeche = entry.Nutzflaeche;
            entity.Nutzeinheit = entry.Einheiten;
            entity.Besitzer = Ctx.Kontakte.Find(entry.Besitzer!.Id)!;

            SetOptionalValues(entity, entry);
            Ctx.Wohnungen.Update(entity);
            Ctx.SaveChanges();

            return new WohnungEntry(entity);
        }

        private void SetOptionalValues(Wohnung entity, WohnungEntry entry)
        {
            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
        }
    }
}
