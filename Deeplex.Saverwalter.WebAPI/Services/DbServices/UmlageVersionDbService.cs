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
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class UmlageVersionDbService : WalterDbServiceBase<UmlageVersionEntry, int, UmlageVersion>
    {
        public UmlageVersionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<UmlageVersion>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.UmlageVersionen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<UmlageVersionEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new UmlageVersionEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.UmlageVersionen.Remove(entity);
                await Ctx.SaveChangesAsync();
                return new OkResult();
            });
        }

        public override async Task<ActionResult<UmlageVersionEntry>> Post(ClaimsPrincipal user, UmlageVersionEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage!.Id);
                var authRx = await Auth.AuthorizeAsync(user, umlage, [Operations.SubCreate]);
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

        private async Task<UmlageVersionEntry> Add(UmlageVersionEntry entry)
        {
            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage!.Id);
            var schluessel = (Umlageschluessel)entry.Schluessel.Id;
            var entity = new UmlageVersion(entry.Beginn, schluessel)
            {
                Umlage = umlage!
            };

            SetOptionalValues(entity, entry);
            Ctx.UmlageVersionen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new UmlageVersionEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<UmlageVersionEntry>> Put(ClaimsPrincipal user, int id, UmlageVersionEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Beginn = entry.Beginn;
                entity.Schluessel = (Umlageschluessel)entry.Schluessel.Id;

                SetOptionalValues(entity, entry);
                Ctx.UmlageVersionen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new UmlageVersionEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(UmlageVersion entity, UmlageVersionEntry entry)
        {
            if (entity.UmlageVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
