using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : IControllerService<UmlageEntry>
    {
        public SaverwalterContext Ctx { get; }

        public UmlageDbService(SaverwalterContext dbService)
        {
            Ctx = dbService;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new UmlageEntry(entity, Ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Umlagen.Remove(entity);
            Ctx.SaveChanges();

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
            Ctx.Umlagen.Add(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, Ctx);
        }

        public IActionResult Put(int id, UmlageEntry entry)
        {
            var entity = Ctx.Umlagen.Find(id);
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
            Ctx.Umlagen.Update(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, Ctx);
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
                    .AddRange(l
                    .Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId.ToString()))
                    .Select(w => Ctx.Wohnungen.Find(int.Parse(w.Id))!));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId.ToString()));
            }

            if (entry.SelectedZaehler is IEnumerable<SelectionEntry> zaehler)
            {
                // Add missing zaehler
                entity.Zaehler.AddRange(zaehler
                    .Where(z => !entity.Zaehler.Exists(e => z.Id == e.ZaehlerId.ToString()))
                    .Select(w => Ctx.ZaehlerSet.Find(int.Parse(w.Id))!));
                // Remove old zaehler
                entity.Zaehler.RemoveAll(w => !zaehler.ToList().Exists(e => e.Id == w.ZaehlerId.ToString()));
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
