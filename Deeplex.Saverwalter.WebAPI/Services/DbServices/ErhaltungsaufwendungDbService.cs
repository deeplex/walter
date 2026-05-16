// Copyright (c) 2023-2025 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : WalterDbServiceBase<ErhaltungsaufwendungEntry, int, Erhaltungsaufwendung>
    {
        public ErhaltungsaufwendungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<ErhaltungsaufwendungEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            ErhaltungsaufwendungPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Bezeichnung.ToLower().Contains(t) ||
                    e.Aussteller.Name.ToLower().Contains(t) ||
                    (e.Aussteller.Vorname != null && e.Aussteller.Vorname.ToLower().Contains(t)) ||
                    e.Wohnung.Bezeichnung.ToLower().Contains(t) ||
                    (e.Wohnung.Adresse != null && e.Wohnung.Adresse.Stadt.ToLower().Contains(t)),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "betrag" => q.SortBy(e => e.Betrag, dir),
                    "bezeichnung" => q.SortBy(e => e.Bezeichnung, dir),
                    _ => q.SortBy(e => e.Datum, dir)
                },
                toEntry: async e => new ErhaltungsaufwendungEntryBase(e, await GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Erhaltungsaufwendung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Erhaltungsaufwendungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<ErhaltungsaufwendungEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new ErhaltungsaufwendungEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Erhaltungsaufwendungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<ErhaltungsaufwendungEntry>> Post(ClaimsPrincipal user, ErhaltungsaufwendungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
                var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<ErhaltungsaufwendungEntry> Add(ErhaltungsaufwendungEntry entry)
        {
            var aussteller = (await Ctx.Kontakte.FindAsync(entry.Aussteller.Id))!;
            var wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;
            var entity = new Erhaltungsaufwendung(entry.Betrag, entry.Bezeichnung, entry.Datum)
            {
                Aussteller = aussteller,
                Wohnung = wohnung,
            };

            SetOptionalValues(entity, entry);
            Ctx.Erhaltungsaufwendungen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new ErhaltungsaufwendungEntry(entity, new());
        }

        public override async Task<ActionResult<ErhaltungsaufwendungEntry>> Put(ClaimsPrincipal user, int id, ErhaltungsaufwendungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Betrag = entry.Betrag;
                entity.Wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;
                entity.Bezeichnung = entry.Bezeichnung;
                entity.Aussteller = (await Ctx.Kontakte.FindAsync(entry.Aussteller.Id))!;
                entity.Datum = entry.Datum;

                SetOptionalValues(entity, entry);
                Ctx.Erhaltungsaufwendungen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new ErhaltungsaufwendungEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Erhaltungsaufwendung entity, ErhaltungsaufwendungEntry entry)
        {
            if (entity.ErhaltungsaufwendungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
