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
using Deeplex.Saverwalter.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageController;
using static Deeplex.Saverwalter.WebAPI.Helper.Utils;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class GarageDbService : WalterDbServiceBase<GarageEntry, int, Garage>
    {
        public GarageDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<GarageEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            GaragePermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Kennung.ToLower().Contains(t) ||
                    (e.Besitzer != null && e.Besitzer.Name.ToLower().Contains(t)) ||
                    (e.Adresse != null && (
                        e.Adresse.Strasse.ToLower().Contains(t) ||
                        e.Adresse.Stadt.ToLower().Contains(t))),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "kennung" => q.SortBy(e => e.Kennung, dir),
                    _ => q.SortBy(e => e.Kennung, dir)
                },
                toEntry: async e => new GarageEntryBase(e, await GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Garage>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Garagen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<GarageEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await GetPermissions(user, entity, Auth);
                return new GarageEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Garagen.Remove(entity);
                await Ctx.SaveChangesAsync();
                return new OkResult();
            });
        }

        public override async Task<ActionResult<GarageEntry>> Post(ClaimsPrincipal user, GarageEntry entry)
        {
            if (entry.Id != 0) return new BadRequestResult();

            try
            {
                var besitzer = await Ctx.Kontakte.FindAsync(entry.Besitzer?.Id);
                if (besitzer == null) return new BadRequestResult();

                var idx = $"G{entry.Id:D5}";
                var entity = new Garage(entry.Kennung)
                {
                    Besitzer = besitzer,
                    Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null,
                    Notiz = entry.Notiz,
                    Ertragskonto = new Buchungskonto($"{idx}-EK", "Garagenmietertrag", BuchungskontoTyp.Ertrag)
                };

                Ctx.Garagen.Add(entity);
                Ctx.SaveChanges();

                return new GarageEntry(entity, entry.Permissions);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public override async Task<ActionResult<GarageEntry>> Put(ClaimsPrincipal user, int id, GarageEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Kennung = entry.Kennung;

                if (entry.Besitzer != null)
                    entity.Besitzer = (await Ctx.Kontakte.FindAsync(entry.Besitzer.Id))!;

                entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
                entity.Notiz = entry.Notiz;

                Ctx.Garagen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new GarageEntry(entity, entry.Permissions);
            });
        }
    }
}
