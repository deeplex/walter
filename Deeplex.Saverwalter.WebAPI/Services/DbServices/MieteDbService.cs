using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MieteDbService : IControllerService<MieteEntryBase>
    {
        public SaverwalterContext Ctx { get; }

        public MieteDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new MieteEntryBase(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Mieten.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Mieten.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(MieteEntryBase entry)
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

        private MieteEntryBase Add(MieteEntryBase entry)
        {
            var mieten = new List<Miete>();

            // Be able to create multiple Mieten at once
            for (int i = 0; i <= entry.Repeat; ++i)
            {
                var vertrag = Ctx.Vertraege.Find(int.Parse(entry.Vertrag.Id));
                var monat = entry.BetreffenderMonat.AddMonths(i);
                var entity = new Miete(entry.Zahlungsdatum, monat, entry.Betrag)
                {
                    Vertrag = vertrag!
                };

                SetOptionalValues(entity, entry);
                Ctx.Mieten.Add(entity);
                mieten.Add(entity);
            }

            Ctx.SaveChanges();

            return new MieteEntryBase(mieten.First(), entry.Repeat);

        }


        public IActionResult Put(int id, MieteEntryBase entry)
        {
            var entity = Ctx.Mieten.Find(id);
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

        private MieteEntryBase Update(MieteEntryBase entry, Miete entity)
        {
            entity.BetreffenderMonat = entry.BetreffenderMonat;
            entity.Betrag = entry.Betrag;
            entity.Zahlungsdatum = entry.Zahlungsdatum;

            SetOptionalValues(entity, entry);
            Ctx.Mieten.Update(entity);
            Ctx.SaveChanges();

            return new MieteEntryBase(entity);
        }

        private void SetOptionalValues(Miete entity, MieteEntryBase entry)
        {
            if (entity.MieteId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
