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
                var entry = new VertragEntry(entity, Ctx);
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
            var wohnung = Ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id));
            var entity = new Vertrag() { Wohnung = wohnung! };

            SetOptionalValues(entity, entry);
            Ctx.Vertraege.Add(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity, Ctx);
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
            entity.Wohnung = Ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id))!;

            SetOptionalValues(entity, entry);
            Ctx.Vertraege.Update(entity);
            Ctx.SaveChanges();

            return new VertragEntry(entity, Ctx);
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
            entity.AnsprechpartnerId = entry.Ansprechpartner is SelectionEntry s ? new Guid(s.Id!) : null;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                // Get a list of all mieter
                var mieter = Ctx.MieterSet.Where(m => m.Vertrag.VertragId == entity.VertragId).ToList();
                // Add missing mieter
                foreach (var selectedMieter in l)
                {
                    if (!mieter.Any(m => m.PersonId.ToString() == selectedMieter.Id))
                    {
                        var newMieter = new Mieter(new Guid(selectedMieter.Id!))
                        {
                            Vertrag = entity
                        };
                        Ctx.MieterSet.Add(newMieter); ;
                    }
                }
                // Remove mieter
                foreach (var m in mieter)
                {
                    if (!l.Any(e => m.PersonId.ToString() == e.Id))
                    {
                        Ctx.MieterSet.Remove(m);
                    }
                }
            }
        }
    }
}
