using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class WohnungDbService : IControllerService<WohnungEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public WohnungDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Wohnung entity, WohnungEntry entry)
        {
            if (entity.WohnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Bezeichnung = entry.Bezeichnung ?? "";
            entity.Wohnflaeche = entry.Wohnflaeche;
            entity.Nutzflaeche = entry.Nutzflaeche;
            entity.Nutzeinheit = entry.Einheiten;
            entity.Notiz = entry.Notiz;
            // TODO guid may be null?
            entity.BesitzerId = entry.Besitzer is SelectionEntry b ? new Guid(b.Id!) : Guid.Empty;
            if (entry.Adresse is AdresseEntry a)
            {
                entity.Adresse = GetAdresse(a, ctx);
            }
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
            var entity = new Wohnung();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Wohnungen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new WohnungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

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
                SetValues(entity, entry);
                DbService.ctx.Wohnungen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new WohnungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
