using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : IControllerService<VertragEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public VertragDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new VertragEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Vertraege.Remove(entity);
            DbService.SaveWalter();

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
            var wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id!));
            var entity = new Vertrag() { Wohnung = wohnung! };

            SetOptionalValues(entity, entry);
            DbService.ctx.Vertraege.Add(entity);
            DbService.SaveWalter();

            return new VertragEntry(entity, DbService);
        }

        public IActionResult Put(int id, VertragEntry entry)
        {
            var entity = DbService.ctx.Vertraege.Find(id);
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
            entity.Wohnung = DbService.ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id))!;

            SetOptionalValues(entity, entry);
            DbService.ctx.Vertraege.Update(entity);
            DbService.SaveWalter();

            return new VertragEntry(entity, DbService);
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
                var mieter = DbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == entity.VertragId).ToList();
                // Add missing mieter
                foreach (var selectedMieter in l)
                {
                    if (!mieter.Any(m => m.PersonId.ToString() == selectedMieter.Id))
                    {
                        var newMieter = new Mieter(new Guid(selectedMieter.Id!))
                        {
                            Vertrag = entity
                        };
                        DbService.ctx.MieterSet.Add(newMieter); ;
                    }
                }
                // Remove mieter
                foreach (var m in mieter)
                {
                    if (!l.Any(e => m.PersonId.ToString() == e.Id))
                    {
                        DbService.ctx.MieterSet.Remove(m);
                    }
                }
            }
        }
    }
}
