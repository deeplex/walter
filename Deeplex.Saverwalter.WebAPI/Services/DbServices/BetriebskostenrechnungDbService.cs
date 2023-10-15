using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : IControllerService<BetriebskostenrechnungEntry>
    {
        public SaverwalterContext Ctx { get; }

        public BetriebskostenrechnungDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new BetriebskostenrechnungEntry(entity, Ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Betriebskostenrechnungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Betriebskostenrechnungen.Remove(entity);
            Ctx.SaveChanges();

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
            if (entry.Umlage == null)
            {
                throw new ArgumentException("entry.Umlage can't be null.");
            }
            var umlage = Ctx.Umlagen.Find(int.Parse(entry.Umlage.Id));
            if (umlage == null)
            {
                throw new ArgumentException($"Did not find Umlage with Id {entry.Umlage.Id}");
            }
            var entity = new Betriebskostenrechnung(entry.Betrag, entry.Datum, entry.BetreffendesJahr)
            {
                Umlage = umlage!
            };

            SetOptionalValues(entity, entry);
            Ctx.Betriebskostenrechnungen.Add(entity);
            Ctx.SaveChanges();

            return new BetriebskostenrechnungEntry(entity, Ctx);
        }

        public IActionResult Put(int id, BetriebskostenrechnungEntry entry)
        {
            var entity = Ctx.Betriebskostenrechnungen.Find(id);
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
            if (entry.Umlage == null)
            {
                throw new ArgumentException("entry has no Umlage");
            }
            var umlage = Ctx.Umlagen.Find(int.Parse(entry.Umlage.Id));
            if (umlage == null)
            {
                throw new ArgumentException($"entry has no Umlage with Id {entry.Umlage.Id}");
            }

            entity.Umlage = umlage;

            SetOptionalValues(entity, entry);
            Ctx.Betriebskostenrechnungen.Update(entity);
            Ctx.SaveChanges();

            return new BetriebskostenrechnungEntry(entity, Ctx);
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
