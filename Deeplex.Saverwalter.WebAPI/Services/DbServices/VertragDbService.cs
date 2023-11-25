using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : IControllerService<VertragEntry>
    {
        public SaverwalterContext Ctx { get; }

        public VertragDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new VertragEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Vertraege.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(VertragEntry entry)
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

        private VertragEntry Add(VertragEntry entry)
        {
            if (entry.Wohnung == null)
            {
                throw new ArgumentException("entry has no Wohnung");
            }
            var wohnung = Ctx.Wohnungen.Find(entry.Wohnung.Id);
            var entity = new Vertrag() { Wohnung = wohnung! };

            SetOptionalValues(entity, entry);
            Ctx.Vertraege.Add(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity);
        }

        public IActionResult Put(int id, VertragEntry entry)
        {
            var entity = Ctx.Vertraege.Find(id);
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

        private VertragEntry Update(VertragEntry entry, Vertrag entity)
        {
            if (entry.Wohnung == null)
            {
                throw new ArgumentException("entry has no Wohnung.");
            }
            entity.Wohnung = Ctx.Wohnungen.Find(entry.Wohnung.Id)!;

            SetOptionalValues(entity, entry);
            Ctx.Vertraege.Update(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity);
        }

        private void SetOptionalValues(Vertrag entity, VertragEntry entry)
        {
            if (entity.VertragId != entry.Id)
            {
                throw new Exception();
            }

            // Create Version for initial create (if provided)
            if (entity.VertragId == 0)
            {
                var entryVersion = entry.Versionen?.FirstOrDefault();
                if (entryVersion != null)
                {
                    var entityVersion = new VertragVersion(entryVersion.Beginn, entryVersion.Grundmiete, entryVersion.Personenzahl)
                    {
                        Vertrag = entity,
                        Notiz = entryVersion.Notiz,
                    };
                    entity.Versionen.Add(entityVersion);
                }
            }

            entity.Ende = entry.Ende;
            entity.Ansprechpartner = Ctx.Kontakte.Find(entry.Ansprechpartner.Id)!;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                // Add missing mieter
                foreach (var selectedMieter in l)
                {
                    if (!entity.Mieter.Any(m => m.KontaktId == selectedMieter.Id))
                    {
                        entity.Mieter.Add(Ctx.Kontakte.Find(selectedMieter.Id)!);
                    }
                }
                // Remove mieter
                foreach (var m in entity.Mieter)
                {
                    if (!l.Any(e => m.KontaktId == e.Id))
                    {
                        entity.Mieter.Remove(m);
                    }
                }
            }
        }
    }
}
