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
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerstandController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ZaehlerstandDbService : WalterDbServiceBase<ZaehlerstandEntry, int, Zaehlerstand>
    {
        public ZaehlerstandDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<Zaehlerstand>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Zaehlerstaende.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<ZaehlerstandEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new ZaehlerstandEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (w) =>
            {
                Ctx.Zaehlerstaende.Remove(w);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<ZaehlerstandEntry>> Post(ClaimsPrincipal user, ZaehlerstandEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var zaehler = await Ctx.ZaehlerSet.FindAsync(entry.Zaehler.Id);
                var authRx = await Auth.AuthorizeAsync(user, zaehler, [Operations.SubCreate]);
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

        private async Task<ZaehlerstandEntry> Add(ZaehlerstandEntry entry)
        {
            var zaehler = await Ctx.ZaehlerSet.FindAsync(entry.Zaehler!.Id!);
            var entity = new Zaehlerstand(entry.Datum, entry.Stand)
            {
                Zaehler = zaehler!
            };
            SetOptionalValues(entity, entry);
            Ctx.Zaehlerstaende.Add(entity);
            await Ctx.SaveChangesAsync();

            return new ZaehlerstandEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<ZaehlerstandEntry>> Put(ClaimsPrincipal user, int id, ZaehlerstandEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Datum = entry.Datum;
                entity.Stand = entry.Stand;

                SetOptionalValues(entity, entry);
                Ctx.Zaehlerstaende.Update(entity);
                await Ctx.SaveChangesAsync();

                return new ZaehlerstandEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Zaehlerstand entity, ZaehlerstandEntry entry)
        {
            entity.Stand = entry.Stand;
            entity.Notiz = entry.Notiz;
        }
    }
}
