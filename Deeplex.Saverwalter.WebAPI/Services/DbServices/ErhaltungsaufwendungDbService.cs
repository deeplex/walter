// Copyright (c) 2023-2025 Kai Lawrence
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
using Deeplex.Saverwalter.WebAPI.Services.Buchungen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : WalterDbServiceBase<ErhaltungsaufwendungEntry, int, Erhaltungsaufwendung>
    {
        private readonly ErhaltungsaufwendungBuchungsService _buchungsService;

        public ErhaltungsaufwendungDbService(
            SaverwalterContext ctx,
            IAuthorizationService authorizationService,
            ErhaltungsaufwendungBuchungsService buchungsService)
            : base(ctx, authorizationService)
        {
            _buchungsService = buchungsService;
        }

        public Task<PagedResult<ErhaltungsaufwendungEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            ErhaltungsaufwendungPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Bezeichnung.ToLower().Contains(t) ||
                    e.Aussteller.Name.ToLower().Contains(t) ||
                    (e.Aussteller.Vorname != null && e.Aussteller.Vorname.ToLower().Contains(t)) ||
                    e.Wohnung.Bezeichnung.ToLower().Contains(t) ||
                    (e.Wohnung.Adresse != null && e.Wohnung.Adresse.Stadt.ToLower().Contains(t)),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "betrag" => q.SortBy(e => e.Betrag, dir),
                    "bezeichnung" => q.SortBy(e => e.Bezeichnung, dir),
                    _ => q.SortBy(e => e.Datum, dir)
                },
                toEntry: async e => {
                    var perms = await GetPermissions(user, e, Auth);
                    perms.Update = false;
                    return new ErhaltungsaufwendungEntryBase(e, perms);
                });

        public override async Task<ActionResult<Erhaltungsaufwendung>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Erhaltungsaufwendungen
                .Include(e => e.Buchungssatz).ThenInclude(s => s!.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(e => e.Wohnung).ThenInclude(w => w.Adresse)
                .Include(e => e.Aussteller)
                .FirstOrDefaultAsync(e => e.ErhaltungsaufwendungId == id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<ErhaltungsaufwendungEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                permissions.Update = false;
                var entry = new ErhaltungsaufwendungEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                if (entity.Buchungssatz != null)
                    return new ConflictObjectResult(
                        "Diese Erhaltungsaufwendung hat einen verknüpften Buchungssatz. Bitte zuerst den Buchungssatz stornieren.");

                Ctx.Erhaltungsaufwendungen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<ErhaltungsaufwendungEntry>> Post(ClaimsPrincipal user, ErhaltungsaufwendungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
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

        public override Task<ActionResult<ErhaltungsaufwendungEntry>> Put(ClaimsPrincipal user, int id, ErhaltungsaufwendungEntry entry)
            => Task.FromResult<ActionResult<ErhaltungsaufwendungEntry>>(new StatusCodeResult(405));

        private async Task<ErhaltungsaufwendungEntry> Add(ErhaltungsaufwendungEntry entry)
        {
            var aussteller = await Ctx.Kontakte
                .Include(k => k.VerbindlichkeitsKonto)
                .FirstOrDefaultAsync(k => k.KontaktId == entry.Aussteller.Id)
                ?? throw new ArgumentException($"Kontakt {entry.Aussteller.Id} nicht gefunden.");
            var wohnung = await Ctx.Wohnungen
                .Include(w => w.AufwandsKonto)
                .FirstOrDefaultAsync(w => w.WohnungId == entry.Wohnung.Id)
                ?? throw new ArgumentException($"Wohnung {entry.Wohnung.Id} nicht gefunden.");

            var entity = await _buchungsService.BucheErhaltungsaufwendungAsync(
                wohnung, aussteller, entry.Betrag, entry.Datum, entry.Bezeichnung, entry.Notiz);

            return new ErhaltungsaufwendungEntry(entity, new());
        }

    }
}
