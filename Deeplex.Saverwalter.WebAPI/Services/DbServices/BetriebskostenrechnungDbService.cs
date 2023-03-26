using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : IControllerService<BetriebskostenrechnungEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public BetriebskostenrechnungDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private BetriebskostenrechnungEntry Add(BetriebskostenrechnungEntry entry)
        {
            var umlage = DbService.ctx.Umlagen.Find(int.Parse(entry.Umlage.Id!));
            var entity = new Betriebskostenrechnung(entry.Betrag, entry.Datum, entry.BetreffendesJahr)
            {
                Umlage = umlage!
            };

            SetOptionalValues(entity, entry);
            DbService.ctx.Betriebskostenrechnungen.Add(entity);
            DbService.SaveWalter();

            return new BetriebskostenrechnungEntry(entity, DbService);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private BetriebskostenrechnungEntry Update(BetriebskostenrechnungEntry entry, Betriebskostenrechnung entity)
        {
            entity.Betrag = entry.Betrag;
            entity.Datum = entry.Datum;
            entity.BetreffendesJahr = entry.BetreffendesJahr;
            entity.Umlage = DbService.ctx.Umlagen.Find(int.Parse(entry.Umlage.Id))!;

            SetOptionalValues(entity, entry);
            DbService.ctx.Betriebskostenrechnungen.Update(entity);
            DbService.SaveWalter();

            return new BetriebskostenrechnungEntry(entity, DbService);
        }

        private void SetOptionalValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
