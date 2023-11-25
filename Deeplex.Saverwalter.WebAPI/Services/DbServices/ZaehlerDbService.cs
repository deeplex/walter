using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerDbService : IControllerService<ZaehlerEntry>
    {
        public SaverwalterContext Ctx { get; }

        public ZaehlerDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.ZaehlerSet.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ZaehlerEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.ZaehlerSet.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.ZaehlerSet.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(ZaehlerEntry entry)
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

        private ZaehlerEntry Add(ZaehlerEntry entry)
        {
            var typ = (Zaehlertyp)entry.Typ.Id;
            var entity = new Zaehler(entry.Kennnummer, typ);

            SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity);
        }

        public IActionResult Put(int id, ZaehlerEntry entry)
        {
            var entity = Ctx.ZaehlerSet.Find(id);
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

        private ZaehlerEntry Update(ZaehlerEntry entry, Zaehler entity)
        {
            entity.Kennnummer = entry.Kennnummer;
            entity.Typ = (Zaehlertyp)entry.Typ.Id;

            SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Update(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity);
        }

        private void SetOptionalValues(Zaehler entity, ZaehlerEntry entry)
        {
            entity.Wohnung = entry.Wohnung is SelectionEntry w ? Ctx.Wohnungen.Find(w.Id) : null;

            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
            entity.Ende = entry.Ende;

            if (entry.SelectedUmlagen is IEnumerable<SelectionEntry> umlagen)
            {
                // Add missing umlagen
                entity.Umlagen.AddRange(umlagen
                    .Where(umlage => !entity.Umlagen.Exists(e => umlage.Id == e.UmlageId))
                    .Select(w => Ctx.Umlagen.Find(w.Id)!));
                // Remove old umlagen
                entity.Umlagen.RemoveAll(w => !umlagen.ToList().Exists(e => e.Id == w.UmlageId));
            }
        }
    }
}
