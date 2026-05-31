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
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.GarageVertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class GarageVertragDbService : WalterDbServiceBase<GarageVertragEntry, int, GarageVertrag>
    {
        public GarageVertragDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<GarageVertragEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            GarageVertragPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Garage.Kennung.ToLower().Contains(t) ||
                    e.Mieter.Any(m => m.Name.ToLower().Contains(t)) ||
                    (e.Vertrag != null && e.Vertrag.Mieter.Any(m => m.Name.ToLower().Contains(t))),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "beginn" => q.SortBy(e => e.Versionen.Min(v => v.Beginn), dir),
                    "ende" => q.SortBy(e => e.Ende, dir),
                    _ => q.SortBy(e => e.Versionen.Min(v => v.Beginn), "desc")
                },
                toEntry: async e => new GarageVertragEntryBase(e, await GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<GarageVertrag>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.GarageVertraege.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<GarageVertragEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await GetPermissions(user, entity, Auth);
                return new GarageVertragEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.GarageVertraege.Remove(entity);
                await Ctx.SaveChangesAsync();
                return new OkResult();
            });
        }

        public override async Task<ActionResult<GarageVertragEntry>> Post(ClaimsPrincipal user, GarageVertragEntry entry)
        {
            if (entry.Id != 0) return new BadRequestResult();

            try
            {
                var garage = await Ctx.Garagen.FindAsync(entry.Garage?.Id);
                if (garage == null) return new BadRequestResult();

                var authRx = await Auth.AuthorizeAsync(user, garage, [Operations.SubCreate]);
                if (!authRx.Succeeded) return new ForbidResult();

                var beginn = entry.Versionen?.FirstOrDefault()?.Beginn;
                if (beginn.HasValue)
                {
                    var conflict = await FindOverlappingGarageVertrag(entry.Garage!.Id, beginn.Value, entry.Ende);
                    if (conflict is not null) return new ConflictObjectResult(conflict);
                }

                return await Add(entry, garage);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<GarageVertragEntry> Add(GarageVertragEntry entry, Garage garage)
        {
            var idx = $"GV{entry.Id:D5}";
            var entity = new GarageVertrag
            {
                Garage = garage,
                MietBuchungskonto = new Buchungskonto($"{idx}-MB", "Garagenmiete", BuchungskontoTyp.Aktiv),
                ZahlungsKonto = new Buchungskonto($"{idx}-ZK", "Zahlung", BuchungskontoTyp.Aktiv),
            };

            await SetOptionalValues(entity, entry);
            Ctx.GarageVertraege.Add(entity);
            Ctx.SaveChanges();

            return new GarageVertragEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<GarageVertragEntry>> Put(ClaimsPrincipal user, int id, GarageVertragEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                if (entry.Garage == null) return new BadRequestResult();

                var beginn = await Ctx.GarageVertragVersionen
                    .Where(v => v.GarageVertrag.GarageVertragId == id)
                    .MinAsync(v => (DateOnly?)v.Beginn);
                if (beginn.HasValue)
                {
                    var conflict = await FindOverlappingGarageVertrag(entry.Garage.Id, beginn.Value, entry.Ende, excludeId: id);
                    if (conflict is not null) return new ConflictObjectResult(conflict);
                }

                entity.Garage = (await Ctx.Garagen.FindAsync(entry.Garage.Id))!;
                await SetOptionalValues(entity, entry);
                Ctx.GarageVertraege.Update(entity);
                await Ctx.SaveChangesAsync();

                return new GarageVertragEntry(entity, entry.Permissions);
            });
        }

        private async Task<string?> FindOverlappingGarageVertrag(int garageId, DateOnly beginn, DateOnly? ende, int excludeId = 0)
        {
            var existing = await Ctx.GarageVertraege
                .Where(v => v.Garage.GarageId == garageId && v.GarageVertragId != excludeId)
                .Select(v => new
                {
                    Ende = v.Ende,
                    Beginn = v.Versionen.Min(vv => (DateOnly?)vv.Beginn),
                    Mieter = v.Mieter.Select(m => m.Bezeichnung),
                    VertragMieter = v.Vertrag != null ? v.Vertrag.Mieter.Select(m => m.Bezeichnung) : Enumerable.Empty<string>()
                })
                .ToListAsync();

            var conflict = existing.FirstOrDefault(v =>
                v.Beginn.HasValue &&
                beginn <= (v.Ende ?? DateOnly.MaxValue) &&
                (ende ?? DateOnly.MaxValue) >= v.Beginn.Value);

            if (conflict is null) return null;

            var mieter = conflict.Mieter.Any()
                ? string.Join(", ", conflict.Mieter)
                : string.Join(", ", conflict.VertragMieter);
            var endeText = conflict.Ende.HasValue ? conflict.Ende.Value.ToString("dd.MM.yyyy") : "offen";
            return $"Konflikt mit bestehendem Garagenvertrag ({mieter}) vom {conflict.Beginn!.Value:dd.MM.yyyy} bis {endeText}.";
        }

        private async Task SetOptionalValues(GarageVertrag entity, GarageVertragEntry entry)
        {
            entity.Ende = entry.Ende;
            entity.Notiz = entry.Notiz;

            entity.Vertrag = entry.Vertrag != null
                ? await Ctx.Vertraege.FindAsync(entry.Vertrag.Id)
                : null;

            if (entity.GarageVertragId == 0)
            {
                var entryVersion = entry.Versionen?.FirstOrDefault();
                if (entryVersion != null)
                {
                    var entityVersion = new GarageVertragVersion(entryVersion.Beginn, entryVersion.GaragenMiete)
                    {
                        GarageVertrag = entity
                    };
                    entity.Versionen.Add(entityVersion);
                }
            }

            if (entry.SelectedMieter is IEnumerable<SelectionEntry> l)
            {
                foreach (var selected in l)
                {
                    if (!entity.Mieter.Any(m => m.KontaktId == selected.Id))
                        entity.Mieter.Add((await Ctx.Kontakte.FindAsync(selected.Id))!);
                }
                foreach (var m in entity.Mieter.ToList())
                {
                    if (!l.Any(e => m.KontaktId == e.Id))
                        entity.Mieter.Remove(m);
                }
            }
        }
    }
}
