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
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MietminderungDbService : WalterDbServiceBase<MietminderungEntry, Mietminderung>
    {
        public MietminderungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<MietminderungEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await MietminderungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new MietminderungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Mietminderung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<MietminderungEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new MietminderungEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Mietminderungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<MietminderungEntry>> Post(ClaimsPrincipal user, MietminderungEntry entry)
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

        private async Task<MietminderungEntry> Add(MietminderungEntry entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag.Id);
            var entity = new Mietminderung(entry.Beginn, entry.Minderung)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.Mietminderungen.Add(entity);
            Ctx.SaveChanges();

            return new MietminderungEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<MietminderungEntry>> Put(ClaimsPrincipal user, int id, MietminderungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Ende = entry.Ende;
                entity.Beginn = entry.Beginn;
                entity.Minderung = entry.Minderung;

                SetOptionalValues(entity, entry);
                Ctx.Mietminderungen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new MietminderungEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Mietminderung entity, MietminderungEntry entry)
        {
            if (entity.MietminderungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
