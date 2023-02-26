using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : BaseDbService<BetriebskostenrechnungEntry>, IControllerService<BetriebskostenrechnungEntry>
    {
        private void SetValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Betrag = entry.Betrag;
            entity.BetreffendesJahr = entry.BetreffendesJahr;
            entity.Datum = entry.Datum;
            entity.Notiz = entry.Notiz;
            entity.Umlage = Ref.ctx.Umlagen.Find(entry.Umlage.Id);
        }

        public BetriebskostenrechnungDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(int id)
        {
            var entity = Ref.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new BetriebskostenrechnungEntry(entity, Ref);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ref.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ref.ctx.Betriebskostenrechnungen.Remove(entity);

            return Save(null!);
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
                Ref.ctx.Betriebskostenrechnungen.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, BetriebskostenrechnungEntry entry)
        {
            var entity = Ref.ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                Ref.ctx.Betriebskostenrechnungen.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
