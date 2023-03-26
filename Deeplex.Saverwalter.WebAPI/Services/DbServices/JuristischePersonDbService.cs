using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.JuristischePersonController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class JuristischePersonDbService : IControllerService<JuristischePersonEntry>
    {
        public WalterDbService.WalterDb DbService { get; }
        public SaverwalterContext ctx => DbService.ctx;

        public JuristischePersonDbService(WalterDbService.WalterDb dbService)
        {
            DbService = dbService;
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

            try
            {
                return new OkObjectResult(Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private JuristischePersonEntry Add(JuristischePersonEntry entry)
        {
            var entity = new JuristischePerson(entry.Name);

            SetOptionalValues(entity, entry);
            DbService.ctx.JuristischePersonen.Add(entity);
            DbService.SaveWalter();

            return new JuristischePersonEntry(entity, DbService);
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
                return new OkObjectResult(Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private JuristischePersonEntry Update(JuristischePersonEntry entry, JuristischePerson entity)
        {
            entity.Bezeichnung = entry.Name;

            SetOptionalValues(entity, entry);
            DbService.ctx.JuristischePersonen.Update(entity);
            DbService.SaveWalter();

            return new JuristischePersonEntry(entity, DbService);
        }

        private void SetOptionalValues(JuristischePerson entity, JuristischePersonEntry entry)
        {
            // NOTE: There is a minus before entry.Id.
            // This is, because FrontEnd needs different Ids for List with Nat. Pers.
            if (entity.JuristischePersonId != -entry.Id)
            {
                throw new Exception();
            }

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
                entity.NatuerlicheMitglieder.RemoveAll((w) => !m.ToList().Exists(e => e.Id == w.PersonId.ToString()));
                entity.JuristischeMitglieder.RemoveAll((w) => !m.ToList().Exists(e => e.Id == w.PersonId.ToString()));
            }
        }
    }
}
