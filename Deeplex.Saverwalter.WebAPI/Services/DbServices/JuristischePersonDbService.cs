using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;
using Deeplex.Saverwalter.WebAPI.Controllers;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class JuristischePersonDbService : IControllerService<JuristischePersonEntry>
    {
        public IWalterDbService DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public JuristischePersonDbService(IWalterDbService dbService)
        {
            DbService = dbService;
        }

        private void SetValues(JuristischePerson entity, JuristischePersonEntry entry)
        {
            if (entity.JuristischePersonId != entry.Id)
            {
                throw new Exception();
            }

            entity.Bezeichnung = entry.Name!;
            entity.Email = entry.Email;
            entity.Fax = entry.Fax;
            entity.Notiz = entry.Notiz;
            entity.Telefon = entry.Telefon;
            entity.Mobil = entry.Mobil;
            if (entry.Adresse is AdresseEntry a)
            {
                entity.Adresse = GetAdresse(a, ctx);
            }
        }

        public IActionResult Get(int id)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new JuristischePersonEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.JuristischePersonen.Remove(entity);
            DbService.SaveWalter();

            return new OkResult();
        }

        public IActionResult Post(JuristischePersonEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }
            var entity = new JuristischePerson();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.JuristischePersonen.Add(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new JuristischePersonEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(int id, JuristischePersonEntry entry)
        {
            var entity = DbService.ctx.JuristischePersonen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.JuristischePersonen.Update(entity);
                DbService.SaveWalter();

                return new OkObjectResult(new JuristischePersonEntry(entity, DbService));
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
