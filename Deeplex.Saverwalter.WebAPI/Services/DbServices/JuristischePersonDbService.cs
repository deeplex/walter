using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;
using Deeplex.Saverwalter.WebAPI.Controllers;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

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
            // NOTE: There is a minus before entry.Id.
            // This is, because FrontEnd needs different Ids for List with Nat. Pers.
            if (entity.JuristischePersonId != -entry.Id)
            {
                throw new Exception();
            }

            entity.Bezeichnung = entry.Name!;
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
                    .AddRange(l.Where(w => !entity.JuristischePersonen.Exists(e => w.Id == e.PersonId.ToString()))
                    .Select(w => ctx.FindPerson(new Guid(w.Id!)) as JuristischePerson)!);
                // Remove old
                entity.JuristischePersonen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.PersonId.ToString()));
            }
            if (entry.SelectedMitglieder is IEnumerable<SelectionEntry> m)
            {
                // Add new
                var missingPersons = m.Where(w =>
                    !entity.Mitglieder.Exists(e => w.Id == e.PersonId.ToString())
                    ).Select(w => ctx.FindPerson(new Guid(w.Id!)!)!);

                entity.JuristischeMitglieder.AddRange(missingPersons!
                    .Where(e => e is JuristischePerson)
                    .Select(e => (JuristischePerson)e));

                entity.NatuerlicheMitglieder.AddRange(missingPersons!
                    .Where(e => e is NatuerlichePerson)
                    .Select(e => (NatuerlichePerson)e));

                // Remove old
                entity.Mitglieder.RemoveAll(w => !m.ToList().Exists(e => e.Id == w.PersonId.ToString()));
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
