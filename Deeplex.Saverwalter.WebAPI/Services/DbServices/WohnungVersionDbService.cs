// Copyright (c) 2023-2026 Kai Lawrence
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
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class WohnungVersionDbService : WalterDbServiceBase<WohnungVersionEntry, int, WohnungVersion>
    {
        public WohnungVersionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<WohnungVersion>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.WohnungVersionen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<WohnungVersionEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new WohnungVersionEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.WohnungVersionen.Remove(entity);
                await Ctx.SaveChangesAsync();
                return new OkResult();
            });
        }

        public override async Task<ActionResult<WohnungVersionEntry>> Post(ClaimsPrincipal user, WohnungVersionEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung!.Id);
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

        private async Task<WohnungVersionEntry> Add(WohnungVersionEntry entry)
        {
            var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung!.Id);
            var entity = new WohnungVersion(
                entry.Beginn,
                entry.Wohnflaeche,
                entry.Nutzflaeche,
                entry.Miteigentumsanteile,
                entry.Einheiten)
            {
                Wohnung = wohnung!
            };

            SetOptionalValues(entity, entry);
            Ctx.WohnungVersionen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new WohnungVersionEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<WohnungVersionEntry>> Put(ClaimsPrincipal user, int id, WohnungVersionEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Beginn = entry.Beginn;
                entity.Wohnflaeche = entry.Wohnflaeche;
                entity.Nutzflaeche = entry.Nutzflaeche;
                entity.Miteigentumsanteile = entry.Miteigentumsanteile;
                entity.Nutzeinheit = entry.Einheiten;

                SetOptionalValues(entity, entry);
                Ctx.WohnungVersionen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new WohnungVersionEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(WohnungVersion entity, WohnungVersionEntry entry)
        {
            if (entity.WohnungVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
