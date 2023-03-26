using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : IControllerService<UmlageEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public UmlageDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private UmlageEntry Add(UmlageEntry entry)
        {
            var typ = (Betriebskostentyp)int.Parse(entry.Typ.Id);
            var schluessel = (Umlageschluessel)int.Parse(entry.Schluessel.Id);
            var entity = new Umlage(typ, schluessel);

            SetOptionalValues(entity, entry);
            DbService.ctx.Umlagen.Add(entity);
            DbService.SaveWalter();

            return new UmlageEntry(entity, DbService);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private UmlageEntry Update(UmlageEntry entry, Umlage entity)
        {
            entity.Typ = (Betriebskostentyp)int.Parse(entry.Typ.Id);
            entity.Schluessel = (Umlageschluessel)int.Parse(entry.Schluessel.Id);

            SetOptionalValues(entity, entry);
            DbService.ctx.Umlagen.Update(entity);
            DbService.SaveWalter();

            return new UmlageEntry(entity, DbService);
        }

        private void SetOptionalValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beschreibung = entry.Beschreibung;
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
    }
}
