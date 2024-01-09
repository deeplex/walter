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
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontaktController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AdresseDbService : WalterDbServiceBase<AdresseEntry, Adresse>
    {
        public AdresseDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<AdresseEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await AdressePermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new AdresseEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Adresse>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Adressen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<AdresseEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new AdresseEntry(entity, permissions);

                entry.Wohnungen = await Task.WhenAll(entity.Wohnungen
                    .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Kontakte = await Task.WhenAll(entity.Kontakte
                    .Select(async e => new KontaktEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Zaehler = await Task.WhenAll(entity.Zaehler
                    .Select(async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Adressen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<AdresseEntry>> Post(ClaimsPrincipal user, AdresseEntry entry)
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

        private AdresseEntry Add(AdresseEntry entry)
        {
            var entity = new Adresse(entry.Strasse, entry.Hausnummer, entry.Postleitzahl, entry.Stadt);
            SetOptionalValues(entity, entry);
            Ctx.Adressen.Add(entity);
            Ctx.SaveChanges();

            return new AdresseEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<AdresseEntry>> Put(ClaimsPrincipal user, int id, AdresseEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Strasse = entry.Strasse;
                entity.Hausnummer = entry.Hausnummer;
                entity.Postleitzahl = entry.Postleitzahl;
                entity.Stadt = entry.Stadt;

                SetOptionalValues(entity, entry);
                Ctx.Adressen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new AdresseEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Adresse entity, AdresseEntry entry)
        {
            if (entity.AdresseId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
