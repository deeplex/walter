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
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public NatuerlichePersonDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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
            DbService.ctx.NatuerlichePersonen.Add(entity);
            DbService.SaveWalter();

            return new NatuerlichePersonEntry(entity, DbService);
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
            DbService.ctx.NatuerlichePersonen.Update(entity);
            DbService.SaveWalter();

            return new NatuerlichePersonEntry(entity, DbService);
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
    }
}
