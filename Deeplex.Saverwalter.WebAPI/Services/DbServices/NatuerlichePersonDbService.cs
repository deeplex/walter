using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.NatuerlichePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class NatuerlichePersonDbService : IControllerService<NatuerlichePersonEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public NatuerlichePersonDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(NatuerlichePerson entity, NatuerlichePersonEntry entry)
        {
            if (entity.NatuerlichePersonId != entry.Id)
            {
                throw new Exception();
            }

            entity.Vorname = entry.Vorname;
            entity.Nachname = entry.Nachname!;
            entity.Email= entry.Email;
            entity.Fax = entry.Fax;
            entity.Notiz = entry.Notiz;
            entity.Telefon = entry.Telefon;
            entity.Mobil = entry.Mobil;
            if (entry.Adresse is AdresseEntry a)
            {
                entity.Adresse = GetAdresse(a, ctx);
            }
            if (entry.SelectedJuristischePersonen is IEnumerable<SelectionEntry> l)
            {
                // Add new
                entity.JuristischePersonen
                    .AddRange(l.Where(w => !entity.JuristischePersonen.Exists(e => w.Id == e.JuristischePersonId.ToString()))
                    .Select(w => ctx.JuristischePersonen.Find(new Guid(w.Id!))));
                // Remove old
                entity.JuristischePersonen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.JuristischePersonen.ToString()));
            }
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new NatuerlichePersonEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.NatuerlichePersonen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(NatuerlichePersonEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new NatuerlichePerson();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.NatuerlichePersonen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new NatuerlichePersonEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, NatuerlichePersonEntry entry)
        {
            var entity = DbService.ctx.NatuerlichePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.NatuerlichePersonen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new NatuerlichePersonEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
