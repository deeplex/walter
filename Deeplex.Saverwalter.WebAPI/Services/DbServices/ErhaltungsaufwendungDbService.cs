using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : IControllerService<ErhaltungsaufwendungEntry>
    {
        public SaverwalterContext Ctx { get; }

        public ErhaltungsaufwendungDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new ErhaltungsaufwendungEntry(entity, Ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Erhaltungsaufwendungen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Erhaltungsaufwendungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(ErhaltungsaufwendungEntry entry)
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

        private ErhaltungsaufwendungEntry Add(ErhaltungsaufwendungEntry entry)
        {
            var ausstellerId = new Guid(entry.Aussteller.Id);
            var wohnung = Ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id));
            var entity = new Erhaltungsaufwendung(entry.Betrag, entry.Bezeichnung, ausstellerId, entry.Datum)
            {
                Wohnung = wohnung!
            };

            SetOptionalValues(entity, entry);
            Ctx.Erhaltungsaufwendungen.Add(entity);
            Ctx.SaveChanges();

            return new ErhaltungsaufwendungEntry(entity, Ctx);
        }

        public IActionResult Put(int id, ErhaltungsaufwendungEntry entry)
        {
            var entity = Ctx.Erhaltungsaufwendungen.Find(id);
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

        private ErhaltungsaufwendungEntry Update(ErhaltungsaufwendungEntry entry, Erhaltungsaufwendung entity)
        {
            entity.Betrag = entry.Betrag;
            entity.Wohnung = Ctx.Wohnungen.Find(int.Parse(entry.Wohnung.Id))!;
            entity.Bezeichnung = entry.Bezeichnung;
            entity.AusstellerId = new Guid(entry.Aussteller.Id);
            entity.Datum = entry.Datum;

            SetOptionalValues(entity, entry);
            Ctx.Erhaltungsaufwendungen.Update(entity);
            Ctx.SaveChanges();

            return new ErhaltungsaufwendungEntry(entity, Ctx);
        }

        private void SetOptionalValues(Erhaltungsaufwendung entity, ErhaltungsaufwendungEntry entry)
        {
            if (entity.ErhaltungsaufwendungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
