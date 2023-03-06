using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragDbService : IControllerService<VertragEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public VertragDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Vertrag entity, VertragEntry entry)
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
                    var entityVersion = new VertragVersion()
                    {
                        Beginn = (DateTime)entryVersion.Beginn!,
                        Grundmiete = entryVersion.Grundmiete,
                        Personenzahl = entryVersion.Personenzahl,
                        Notiz = entryVersion.Notiz,
                        Vertrag = entity
                    };
                    entity.Versionen.Add(entityVersion);
                }
            }

            entity.Ende = entry.Ende;
            if (entry.Wohnung is SelectionEntry w)
            {
                entity.Wohnung = DbService.ctx.Wohnungen.Find(int.Parse(w.Id!));
            }
            else
            {
                throw new Exception();
            }
            entity.AnsprechpartnerId = entry.Ansprechpartner is SelectionEntry s ? new Guid(s.Id!) : null;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                // Get a list of all mieter
                var mieter = DbService.ctx.MieterSet.Where(m => m.Vertrag.VertragId == entity.VertragId).ToList();
                // Add missing mieter
                foreach(var selectedMieter in l)
                {
                    if (!mieter.Any(m => m.PersonId.ToString() == selectedMieter.Id))
                    {
                        DbService.ctx.MieterSet.Add(new Mieter() { PersonId = new Guid(selectedMieter.Id!), Vertrag = entity });
                    }
                }
                // Remove mieter
                foreach(var m in mieter)
                {
                    if (!l.Any(e => m.PersonId.ToString() == e.Id))
                    {
                        DbService.ctx.MieterSet.Remove(m);
                    }
                }
            }
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
            var entity = new Vertrag();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Vertraege.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

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
                SetValues(entity, entry);
                DbService.ctx.Vertraege.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new VertragEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
