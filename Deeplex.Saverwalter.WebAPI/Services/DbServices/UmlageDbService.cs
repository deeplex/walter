using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : IControllerService<UmlageEntry>
    {
        public SaverwalterContext Ctx { get; }

        public UmlageDbService(SaverwalterContext dbService)
        {
            Ctx = dbService;
        }

        public IActionResult Get(int id)
        {
            var entity = Ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new UmlageEntry(entity, Ctx);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = Ctx.Umlagen.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            Ctx.Umlagen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public IActionResult Post(UmlageEntry entry)
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

        private UmlageEntry Add(UmlageEntry entry)
        {
            if (entry.Typ == null)
            {
                throw new ArgumentException("entry has no Typ.");
            }

            if (entry.Schluessel == null)
            {
                throw new ArgumentException("entry has no Schluessel.");
            }
            var schluessel = (Umlageschluessel)int.Parse(entry.Schluessel.Id);
            var typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == int.Parse(entry.Typ.Id));
            var entity = new Umlage(schluessel)
            {
                Typ = typ
            };

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Add(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, Ctx);
        }

        public IActionResult Put(int id, UmlageEntry entry)
        {
            var entity = Ctx.Umlagen.Find(id);
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

        private UmlageEntry Update(UmlageEntry entry, Umlage entity)
        {
            if (entry.Typ == null)
            {
                throw new ArgumentException("entry has no Typ.");
            }
            if (entry.Schluessel == null)
            {
                throw new ArgumentException("entry has no Schluessel.");
            }

            entity.Typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == int.Parse(entry.Typ.Id));
            entity.Schluessel = (Umlageschluessel)int.Parse(entry.Schluessel.Id);

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Update(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, Ctx);
        }

        private void SetOptionalValues(Umlage entity, UmlageEntry entry)
        {
            if (entity.UmlageId != entry.Id)
            {
                throw new Exception();
            }

            entity.Beschreibung = entry.Beschreibung;

            if (entry.SelectedWohnungen is IEnumerable<SelectionEntry> l)
            {
                // Add missing Wohnungen
                entity.Wohnungen
                    .AddRange(l
                    .Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId.ToString()))
                    .Select(w => Ctx.Wohnungen.Find(int.Parse(w.Id))!));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId.ToString()));
            }

            if (entry.SelectedZaehler is IEnumerable<SelectionEntry> zaehler)
            {
                // Add missing zaehler
                entity.Zaehler.AddRange(zaehler
                    .Where(z => !entity.Zaehler.Exists(e => z.Id == e.ZaehlerId.ToString()))
                    .Select(w => Ctx.ZaehlerSet.Find(int.Parse(w.Id))!));
                // Remove old zaehler
                entity.Zaehler.RemoveAll(w => !zaehler.ToList().Exists(e => e.Id == w.ZaehlerId.ToString()));
            }

            if (entry.Schluessel != null &&
                (Umlageschluessel)int.Parse(entry.Schluessel.Id) == Umlageschluessel.NachVerbrauch &&
                entry.HKVO is HKVOEntryBase hkvo)
            {
                if (hkvo.Id == 0)
                {
                    var newHKVO = new HKVO(hkvo.HKVO_P7 / 100, hkvo.HKVO_P8 / 100, (HKVO_P9A2)int.Parse(hkvo.HKVO_P9.Id), hkvo.Strompauschale / 100)
                    {
                        Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == int.Parse(hkvo.Stromrechnung.Id))
                    };
                    entity.HKVO = newHKVO;
                    Ctx.HKVO.Add(newHKVO);
                }
                else
                {
                    var oldHKVO = Ctx.HKVO.Single(e => e.HKVOId == hkvo.Id);
                    oldHKVO.HKVO_P7 = (double)hkvo.HKVO_P7 / 100;
                    oldHKVO.HKVO_P8 = (double)hkvo.HKVO_P8 / 100;
                    oldHKVO.HKVO_P9 = (HKVO_P9A2)int.Parse(hkvo.HKVO_P9.Id);
                    oldHKVO.Strompauschale = (double)hkvo.Strompauschale / 100;
                    oldHKVO.Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == int.Parse(hkvo.Stromrechnung.Id));
                    Ctx.HKVO.Update(oldHKVO);
                }
            }
            else
            {
                entity.HKVO = null;
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
