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
    public class HKVODbService : WalterDbServiceBase<HKVOEntryBase, int, HKVO>
    {
        public HKVODbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<HKVO>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.HKVO.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<HKVOEntryBase>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new HKVOEntryBase(entity, permissions);
            });
        }

        public override async Task<ActionResult<HKVOEntryBase>> Put(ClaimsPrincipal user, int id, HKVOEntryBase entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Beginn = entry.Beginn;
                entity.HKVO_P7 = entry.HKVO_P7 / 100m;
                entity.HKVO_P8 = entry.HKVO_P8 / 100m;
                entity.HKVO_P9 = (HKVO_P9A2)entry.HKVO_P9.Id;
                entity.Strompauschale = entry.Strompauschale / 100m;

                var betriebsstrom = await Ctx.Umlagen.FindAsync(entry.Stromrechnung.Id);
                if (betriebsstrom != null)
                    entity.Betriebsstrom = betriebsstrom;

                entity.AllgemeinWaerme = entry.AllgemeinWaerme != null
                    ? await Ctx.ZaehlerSet.FindAsync(entry.AllgemeinWaerme.Id)
                    : null;

                Ctx.HKVO.Update(entity);
                await Ctx.SaveChangesAsync();

                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new HKVOEntryBase(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.HKVO.Remove(entity);
                await Ctx.SaveChangesAsync();
                return new OkResult();
            });
        }

        public override async Task<ActionResult<HKVOEntryBase>> Post(ClaimsPrincipal user, HKVOEntryBase entry)
        {
            try
            {
                var umlage = await Ctx.Umlagen.FindAsync(entry.UmlageId);
                if (umlage == null) return new NotFoundResult();

                var authRx = await Auth.AuthorizeAsync(user, umlage, [Operations.SubCreate]);
                if (!authRx.Succeeded) return new ForbidResult();

                var betriebsstrom = await Ctx.Umlagen.FindAsync(entry.Stromrechnung.Id);
                if (betriebsstrom == null) return new BadRequestResult();

                var newHkvo = new HKVO(
                    entry.Beginn,
                    entry.HKVO_P7 / 100m,
                    entry.HKVO_P8 / 100m,
                    (HKVO_P9A2)entry.HKVO_P9.Id,
                    entry.Strompauschale / 100m
                )
                {
                    Heizkosten = umlage,
                    Betriebsstrom = betriebsstrom,
                    AllgemeinWaerme = entry.AllgemeinWaerme != null
                        ? await Ctx.ZaehlerSet.FindAsync(entry.AllgemeinWaerme.Id)
                        : null
                };

                Ctx.HKVO.Add(newHkvo);
                await Ctx.SaveChangesAsync();

                var permissions = await Utils.GetPermissions(user, newHkvo, Auth);
                return new HKVOEntryBase(newHkvo, permissions);
            }
            catch
            {
                return new BadRequestResult();
            }
        }
    }
}
