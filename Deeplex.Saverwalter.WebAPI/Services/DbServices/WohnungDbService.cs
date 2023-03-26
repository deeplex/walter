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
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public WohnungDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Wohnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new WohnungEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Wohnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Wohnungen.Remove(entity);
            DbService.SaveWalter();

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
            var entity = new Wohnung(entry.Bezeichnung, entry.Wohnflaeche, entry.Nutzflaeche, entry.Einheiten);

            SetOptionalValues(entity, entry);
            DbService.ctx.Wohnungen.Add(entity);
            DbService.SaveWalter();

            return new WohnungEntry(entity, DbService);
        }

        public IActionResult Put(int id, WohnungEntry entry)
        {
            var entity = DbService.ctx.Wohnungen.Find(id);
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

            SetOptionalValues(entity, entry);
            DbService.ctx.Wohnungen.Update(entity);
            DbService.SaveWalter();

            return new WohnungEntry(entity, DbService);
        }

        private void SetOptionalValues(Wohnung entity, WohnungEntry entry)
        {
            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, ctx) : null;
            entity.Notiz = entry.Notiz;
            entity.BesitzerId = entry.Besitzer is SelectionEntry b ? new Guid(b.Id) : Guid.Empty;
        }
    }
}
