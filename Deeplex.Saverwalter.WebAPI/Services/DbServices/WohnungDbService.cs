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
using Deeplex.Saverwalter.WebAPI.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class WohnungDbService : WalterDbServiceBase<WohnungEntry, Wohnung>
    {
        public WohnungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<WohnungEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await WohnungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Wohnung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Wohnungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<WohnungEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new WohnungEntry(entity, permissions);
                entry.Haus = await Task.WhenAll(entity.Adresse?
                    .Wohnungen.Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))) ?? []);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Wohnungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<WohnungEntry>> Post(ClaimsPrincipal user, WohnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var entity = new Wohnung(
                    entry.Bezeichnung,
                    entry.Wohnflaeche,
                    entry.Nutzflaeche,
                    entry.Miteigentumsanteile,
                    entry.Einheiten);

                var besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer?.Id);
                if (besitzer != null)
                {
                    entity.Besitzer = besitzer;
                }

                SetOptionalValues(entity, entry);
                Ctx.Wohnungen.Add(entity);

                var userId = await Ctx.UserAccounts.FindAsync(user.GetUserId());
                if (userId != null)
                {
                    var verwalterEntity = new Verwalter(VerwalterRolle.Vollmacht)
                    {
                        Wohnung = entity,
                        UserAccount = userId
                    };
                    Ctx.VerwalterSet.Add(verwalterEntity);
                }



                Ctx.SaveChanges();

                return entry;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public override async Task<ActionResult<WohnungEntry>> Put(ClaimsPrincipal user, int id, WohnungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Besitzer != null)
                {
                    entity.Besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer.Id);
                }
                else
                {
                    entity.Besitzer = null;
                }

                entity.Bezeichnung = entry.Bezeichnung;
                entity.Wohnflaeche = entry.Wohnflaeche;
                entity.Nutzflaeche = entry.Nutzflaeche;
                entity.Nutzeinheit = entry.Einheiten;
                entity.Miteigentumsanteile = entry.Miteigentumsanteile;

                SetOptionalValues(entity, entry);
                Ctx.Wohnungen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new WohnungEntry(entity, entry.Permissions);
            });
        }

        private void SetOptionalValues(Wohnung entity, WohnungEntry entry)
        {
            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
        }
    }
}
