using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class KontaktDbService : IControllerService<KontaktEntry>
    {
        public SaverwalterContext Ctx { get; }

        public KontaktDbService(SaverwalterContext ctx)
        {
            Ctx = ctx;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Kontakte.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new KontaktEntry(entity);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Kontakte.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Kontakte.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(KontaktEntry entry)
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

        private KontaktEntry Add(KontaktEntry entry)
        {
            var entity = new Kontakt(entry.Name, (Rechtsform)entry.Rechtsform.Id);

            SetOptionalValues(entity, entry);
            Ctx.Kontakte.Add(entity);
            Ctx.SaveChanges();

            return new KontaktEntry(entity);
        }

        public IActionResult Put(int id, KontaktEntry entry)
        {
            var entity = Ctx.Kontakte.Find(id);
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

        private KontaktEntry Update(KontaktEntry entry, Kontakt entity)
        {
            entity.Name = entry.Name;
            entity.Rechtsform = (Rechtsform)entry.Rechtsform.Id;

            SetOptionalValues(entity, entry);
            Ctx.Kontakte.Update(entity);
            Ctx.SaveChanges();

            return new KontaktEntry(entity);
        }

        private void SetOptionalValues(Kontakt entity, KontaktEntry entry)
        {
            if (entity.KontaktId != entry.Id)
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
                    .AddRange(l.Where(w => !entity.JuristischePersonen.Exists(e => w.Id == e.KontaktId))
                    .Select(w => Ctx.Kontakte.Find(w.Id)!));
                // Remove old
                entity.JuristischePersonen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.KontaktId));
            }

            if (entry.SelectedMitglieder is IEnumerable<SelectionEntry> m)
            {
                // Add new
                entity.Mitglieder
                    .AddRange(m.Where(w => !entity.Mitglieder.Exists(e => w.Id == e.KontaktId))
                    .Select(w => Ctx.Kontakte.Find(w.Id)!));

                // Remove old
                entity.Mitglieder.RemoveAll((w) => !m.ToList().Exists(e => e.Id == w.KontaktId));
            }

            if ((Rechtsform)entry.Rechtsform.Id == Rechtsform.natuerlich)
            {
                if (entry.Anrede is SelectionEntry anrede)
                {
                    entity.Anrede = (Anrede)anrede.Id;
                }
                else
                {
                    throw new ArgumentException("Anrede is required when Rechtsform is Natürlich");
                }
                //entity.Titel = (Titel)(entry.Titel?.Id ?? int.Parse(Titel.Kein));
            }
            else
            {
                entity.Anrede = Anrede.Keine;
            }
        }
    }
}
