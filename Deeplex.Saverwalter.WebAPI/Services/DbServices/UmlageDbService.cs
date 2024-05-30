// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class UmlageDbService : WalterDbServiceBase<UmlageEntry, Umlage>
    {
        public UmlageDbService(SaverwalterContext dbService, IAuthorizationService authorizationService) : base(dbService, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<UmlageEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await UmlagePermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new UmlageEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Umlage>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Umlagen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<UmlageEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new UmlageEntry(entity, permissions);

                entry.Wohnungen = await Task.WhenAll(entity.Wohnungen
                    .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Zaehler = await Task.WhenAll(entity.Zaehler
                    .Select(async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Umlagen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<UmlageEntry>> Post(ClaimsPrincipal user, UmlageEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var wohnungen = entry.Wohnungen!.SelectMany(wohnung => Ctx.Wohnungen.Where(w => w.WohnungId == wohnung.Id));
            var authRx = await Auth.AuthorizeAsync(user, wohnungen, [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                return Add(entry);
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
            var schluessel = (Umlageschluessel)entry.Schluessel.Id;
            var typ = Ctx.Umlagetypen.First(typ => typ.UmlagetypId == entry.Typ.Id);
            var entity = new Umlage(schluessel)
            {
                Typ = typ
            };

            SetOptionalValues(entity, entry);
            Ctx.Umlagen.Add(entity);
            Ctx.SaveChanges();

            return new UmlageEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<UmlageEntry>> Put(ClaimsPrincipal user, int id, UmlageEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Typ == null)
                {
                    throw new ArgumentException("entry has no Typ.");
                }
                if (entry.Schluessel == null)
                {
                    throw new ArgumentException("entry has no Schluessel.");
                }

                entity.Typ = (await Ctx.Umlagetypen.FindAsync(entry.Typ.Id))!;
                entity.Schluessel = (Umlageschluessel)entry.Schluessel.Id;

                SetOptionalValues(entity, entry);
                Ctx.Umlagen.Update(entity);
                Ctx.SaveChanges();

                return new UmlageEntry(entity, entry.Permissions);
            });
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
                entity.Wohnungen.AddRange(l
                    .Where(w => !entity.Wohnungen.Exists(e => w.Id == e.WohnungId))
                    .SelectMany(w => Ctx.Wohnungen.Where(u => u.WohnungId == w.Id)));
                // Remove old Wohnungen
                entity.Wohnungen.RemoveAll(w => !l.ToList().Exists(e => e.Id == w.WohnungId));
            }

            if (entry.SelectedZaehler is IEnumerable<SelectionEntry> zaehler)
            {
                // Add missing zaehler
                entity.Zaehler.AddRange(zaehler
                    .Where(z => !entity.Zaehler.Exists(e => z.Id == e.ZaehlerId))
                    .SelectMany(w => Ctx.ZaehlerSet.Where(u => u.ZaehlerId == w.Id)!));
                // Remove old zaehler
                entity.Zaehler.RemoveAll(w => !zaehler.ToList().Exists(e => e.Id == w.ZaehlerId));
            }

            if (entry.Schluessel != null &&
                (Umlageschluessel)entry.Schluessel.Id == Umlageschluessel.NachVerbrauch &&
                entry.HKVO is HKVOEntryBase hkvo)
            {
                if (hkvo.Id == 0)
                {
                    var newHKVO = new HKVO(
                        ((double)hkvo.HKVO_P7) / 100,
                        ((double)hkvo.HKVO_P8) / 100,
                        (HKVO_P9A2)hkvo.HKVO_P9.Id,
                        ((double)hkvo.Strompauschale) / 100)
                    {
                        Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id)
                    };
                    entity.HKVO = newHKVO;
                    Ctx.HKVO.Add(newHKVO);
                }
                else
                {
                    var oldHKVO = Ctx.HKVO.Single(e => e.HKVOId == hkvo.Id);
                    oldHKVO.HKVO_P7 = ((double)hkvo.HKVO_P7) / 100;
                    oldHKVO.HKVO_P8 = ((double)hkvo.HKVO_P8) / 100;
                    oldHKVO.HKVO_P9 = (HKVO_P9A2)hkvo.HKVO_P9.Id;
                    oldHKVO.Strompauschale = ((double)hkvo.Strompauschale) / 100;
                    oldHKVO.Betriebsstrom = Ctx.Umlagen.Single(e => e.UmlageId == hkvo.Stromrechnung.Id);
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
