using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungControllerService : IControllerService<BetriebskostenrechnungEntry>
    {
        private IWalterDbService DbService { get; }
        private bool SetValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                return false;
            }
            try
            {
                entity.Betrag = entry.Betrag;
                entity.BetreffendesJahr = entry.BetreffendesJahr;
                entity.Datum = entry.Datum;
                entity.Notiz = entry.Notiz;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private IActionResult Save(BetriebskostenrechnungEntry entry)
        {
            try
            {
                DbService.SaveWalter();
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            };
        }

        public BetriebskostenrechnungControllerService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        public IActionResult Get(int id)
        {
            try
            {
                var entry = new BetriebskostenrechnungEntry(DbService.ctx.Betriebskostenrechnungen.Find(id), DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id, BetriebskostenrechnungEntry entry)
        {
            if (id != entry.Id)
            {
                return new BadRequestResult();
            }

            var entity = DbService.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Betriebskostenrechnungen.Remove(entity);

            return Save(entry);
        }

        public IActionResult Post(BetriebskostenrechnungEntry entry)
        {
            var entity = new Betriebskostenrechnung();
            if (!SetValues(entity, entry))
            {
                return new BadRequestResult();
            }
            DbService.ctx.Betriebskostenrechnungen.Add(entity);

            return Save(entry);
        }

        public IActionResult Put(int id, BetriebskostenrechnungEntry entry)
        {
            var entity = DbService.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            if (!SetValues(entity, entry))
            {
                return new BadRequestResult();
            }

            return Save(entry);
        }
    }
}
