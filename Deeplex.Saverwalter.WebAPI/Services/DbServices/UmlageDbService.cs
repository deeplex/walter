using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : IControllerService<UmlageEntry>
    {
        public WalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public UmlageDbService(WalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beschreibung = entry.Beschreibung;
            entity.Typ = (Betriebskostentyp)int.Parse(entry.Typ!.Id!);
            entity.Schluessel = (Umlageschluessel)int.Parse(entry.Schluessel!.Id!);
            if (entry.SelectedWohnungen is IEnumerable<SelectionEntry> l)
            {
                // Add missing Wohnungen
                entity.Wohnungen
                    .AddRange(l.Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId.ToString()))
                    .Select(w => ctx.Wohnungen.Find(int.Parse(w.Id!))));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId.ToString()));
            }
            //entity.Zaehler = entry
            entity.Notiz = entry.Notiz;
        }


        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new UmlageEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Umlagen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(UmlageEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Umlage();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Umlagen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new UmlageEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, UmlageEntry entry)
        {
            var entity = DbService.ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Umlagen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new UmlageEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
