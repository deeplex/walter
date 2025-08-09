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
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragVersionDbService : WalterDbServiceBase<VertragVersionEntry, int, VertragVersion>
    {
        public VertragVersionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<VertragVersion>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<VertragVersionEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new VertragVersionEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.VertragVersionen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<VertragVersionEntry>> Post(ClaimsPrincipal user, VertragVersionEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
                var authRx = await Auth.AuthorizeAsync(user, vertrag, [Operations.SubCreate]);
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

        private async Task<VertragVersionEntry> Add(VertragVersionEntry entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
            var entity = new VertragVersion(entry.Beginn, entry.Grundmiete, entry.Personenzahl)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new VertragVersionEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<VertragVersionEntry>> Put(ClaimsPrincipal user, int id, VertragVersionEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Beginn = entry.Beginn;
                entity.Grundmiete = entry.Grundmiete;
                entity.Personenzahl = entry.Personenzahl;

                SetOptionalValues(entity, entry);
                Ctx.VertragVersionen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new VertragVersionEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(VertragVersion entity, VertragVersionEntry entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
