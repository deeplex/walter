using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : IControllerService<BetriebskostenrechnungEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public BetriebskostenrechnungDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Betrag = entry.Betrag;
            entity.BetreffendesJahr = entry.BetreffendesJahr;
            entity.Datum = (DateTime)entry.Datum!;
            entity.Notiz = entry.Notiz;
            entity.Umlage = DbService.ctx.Umlagen.Find(int.Parse(entry.Umlage!.Id!));
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new BetriebskostenrechnungEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Betriebskostenrechnungen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(BetriebskostenrechnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new Betriebskostenrechnung();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Betriebskostenrechnungen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new BetriebskostenrechnungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, BetriebskostenrechnungEntry entry)
        {
            var entity = DbService.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Betriebskostenrechnungen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new BetriebskostenrechnungEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
