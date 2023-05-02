using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.NatuerlichePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class NatuerlichePersonDbService : IControllerService<NatuerlichePersonEntry>
    {
        public SaverwalterContext Ctx { get; }

        public NatuerlichePersonDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new NatuerlichePersonEntry(entity, Ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.NatuerlichePersonen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(NatuerlichePersonEntry entry)
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

        private NatuerlichePersonEntry Add(NatuerlichePersonEntry entry)
        {
            var entity = new NatuerlichePerson(entry.Nachname);

            SetOptionalValues(entity, entry);
            Ctx.NatuerlichePersonen.Add(entity);
            Ctx.SaveChanges();

            return new NatuerlichePersonEntry(entity, Ctx);
        }

        public IActionResult Put(int id, NatuerlichePersonEntry entry)
        {
            var entity = Ctx.NatuerlichePersonen.Find(id);
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

        private NatuerlichePersonEntry Update(NatuerlichePersonEntry entry, NatuerlichePerson entity)
        {
            entity.Nachname = entry.Nachname;

            SetOptionalValues(entity, entry);
            Ctx.NatuerlichePersonen.Update(entity);
            Ctx.SaveChanges();

            return new NatuerlichePersonEntry(entity, Ctx);
        }

        private void SetOptionalValues(NatuerlichePerson entity, NatuerlichePersonEntry entry)
        {
            if (entity.NatuerlichePersonId != entry.Id)
            {
                throw new Exception();
            }

            entity.Vorname = entry.Vorname;
            entity.Email = entry.Email;
            entity.Fax = entry.Fax;
            entity.Notiz = entry.Notiz;
            entity.Telefon = entry.Telefon;
            entity.Mobil = entry.Mobil;
            if (entry.Adresse is AdresseEntryBase a)
            {
                entity.Adresse = GetAdresse(a, Ctx);
            }
            if (entry.SelectedJuristischePersonen is IEnumerable<SelectionEntry> l)
            {
                // Add new
                entity.JuristischePersonen
                    .AddRange(l.Where(w => !entity.JuristischePersonen.Exists(e => w.Id == e.JuristischePersonId.ToString()))
                    .Select(w => Ctx.JuristischePersonen.Find(new Guid(w.Id))!));
                // Remove old
                entity.JuristischePersonen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.JuristischePersonen.ToString()));
            }
        }
    }
}
