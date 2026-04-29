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
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BetriebskostenrechnungDbService : WalterDbServiceBase<BetriebskostenrechnungEntry, int, Betriebskostenrechnung>
    {
        public BetriebskostenrechnungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<BetriebskostenrechnungEntryBase>>> GetList(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
            {
                var allItems = await Ctx.Betriebskostenrechnungen
                    .AsSplitQuery()
                    .Include(e => e.Umlage).ThenInclude(u => u.Typ)
                    .Include(e => e.Umlage).ThenInclude(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                    .ToListAsync();
                var adminPerms = new Utils.Permissions { Read = true, Update = true, Remove = true };
                return allItems.Select(e => new BetriebskostenrechnungEntryBase(e, adminPerms)).ToArray();
            }

            var userId = user.GetUserId();
            var wohnungAccess = await Ctx.Wohnungen
                .Where(w => w.Verwalter.Any(v => v.UserAccount.Id == userId))
                .Select(w => new
                {
                    w.WohnungId,
                    CanWrite = w.Verwalter.Any(v =>
                        v.UserAccount.Id == userId &&
                        (v.Rolle == VerwalterRolle.Eigentuemer || v.Rolle == VerwalterRolle.Vollmacht))
                })
                .ToListAsync();

            if (wohnungAccess.Count == 0)
            {
                return Array.Empty<BetriebskostenrechnungEntryBase>();
            }

            var readableWohnungIds = wohnungAccess.Select(w => w.WohnungId).ToHashSet();
            var writableWohnungIds = wohnungAccess
                .Where(w => w.CanWrite)
                .Select(w => w.WohnungId)
                .ToHashSet();

            var list = await Ctx.Betriebskostenrechnungen
                .AsSplitQuery()
                .Include(e => e.Umlage).ThenInclude(u => u.Typ)
                .Include(e => e.Umlage).ThenInclude(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Where(e => e.Umlage.Wohnungen.Any(w => readableWohnungIds.Contains(w.WohnungId)))
                .ToListAsync();

            return list.Select(e =>
            {
                var canWrite = e.Umlage.Wohnungen.Any(w => writableWohnungIds.Contains(w.WohnungId));
                return new BetriebskostenrechnungEntryBase(e, new Utils.Permissions { Read = true, Update = canWrite, Remove = canWrite });
            }).ToArray();
        }

        public override async Task<ActionResult<Betriebskostenrechnung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Betriebskostenrechnungen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<BetriebskostenrechnungEntry>> Get(ClaimsPrincipal user, int id)
        {

            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new BetriebskostenrechnungEntry(entity, permissions);

                entry.Betriebskostenrechnungen = await Task.WhenAll(entity.Umlage.Betriebskostenrechnungen
                        .Select(async e => new BetriebskostenrechnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
                entry.Wohnungen = await Task.WhenAll(entity.Umlage.Wohnungen
                        .Select(async e => new WohnungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Betriebskostenrechnungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<BetriebskostenrechnungEntry>> Post(ClaimsPrincipal user, BetriebskostenrechnungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage!.Id);
            var authRx = await Auth.AuthorizeAsync(user, umlage, [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<BetriebskostenrechnungEntry> Add(BetriebskostenrechnungEntry entry)
        {
            if (entry.Umlage == null)
            {
                throw new ArgumentException("entry.Umlage can't be null.");
            }
            var umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id) ?? throw new ArgumentException($"Did not find Umlage with Id {entry.Umlage.Id}");
            var entity = new Betriebskostenrechnung(entry.Betrag, entry.Datum, entry.BetreffendesJahr)
            {
                Umlage = umlage!
            };

            SetOptionalValues(entity, entry);
            Ctx.Betriebskostenrechnungen.Add(entity);
            Ctx.SaveChanges();

            return new BetriebskostenrechnungEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<BetriebskostenrechnungEntry>> Put(ClaimsPrincipal user, int id, BetriebskostenrechnungEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Betrag = entry.Betrag;
                entity.Datum = entry.Datum;
                entity.BetreffendesJahr = entry.BetreffendesJahr;
                if (entry.Umlage == null)
                {
                    throw new ArgumentException("entry has no Umlage");
                }
                entity.Umlage = await Ctx.Umlagen.FindAsync(entry.Umlage.Id) ?? throw new ArgumentException($"entry has no Umlage with Id {entry.Umlage.Id}");

                SetOptionalValues(entity, entry);
                Ctx.Betriebskostenrechnungen.Update(entity);
                Ctx.SaveChanges();

                return new BetriebskostenrechnungEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Betriebskostenrechnung entity, BetriebskostenrechnungEntry entry)
        {
            if (entity.BetriebskostenrechnungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
