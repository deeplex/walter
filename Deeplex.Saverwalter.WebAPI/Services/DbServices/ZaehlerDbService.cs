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
using Deeplex.Saverwalter.WebAPI.Controllers;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Utils.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class ZaehlerDbService : WalterDbServiceBase<ZaehlerEntry, int, Zaehler>
    {
        public ZaehlerDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<ZaehlerEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            ZaehlerPermissionHandler.GetQueryable(Ctx, user)
                .Include(e => e.Adresse)
                .Include(e => e.Wohnung)
                    .ThenInclude(w => w!.Verwalter)
                .Include(e => e.Staende)
                .PagedAsync(query,
                    searchPredicate: t => e =>
                        e.Kennnummer.ToLower().Contains(t) ||
                        (e.Adresse != null && (
                            e.Adresse.Strasse.ToLower().Contains(t) ||
                            e.Adresse.Stadt.ToLower().Contains(t))) ||
                        (e.Wohnung != null && e.Wohnung.Bezeichnung.ToLower().Contains(t)),
                    applySort: (q, sortBy, dir) => sortBy switch
                    {
                        "adresse.anschrift" => q.SortBy(e => e.Adresse!.Stadt, dir)
                            .ThenSortBy(e => e.Adresse!.Strasse, dir),
                        "wohnung.text" => q.SortBy(e => e.Wohnung!.Bezeichnung, dir),
                        "ende" => q.SortBy(e => e.Ende, dir),
                        _ => q.SortBy(e => e.Kennnummer, dir)
                    },
                    toEntry: async e => new ZaehlerEntryBase(e, await Utils.GetPermissions(user, e, Auth)));

        public override async Task<ActionResult<Zaehler>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.ZaehlerSet.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<ZaehlerEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                return new ZaehlerEntry(entity, permissions);
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                // Löschen entfernt die Zählerdaten aus allen Abrechnungen → sperren, wenn
                // für den Zähler überhaupt ein Jahr abgerechnet ist.
                var sperre = await AbrechnungsschutzService.SperreZaehlerAbJahr(Ctx, id, 0);
                if (sperre != null) return new ConflictObjectResult(sperre);

                Ctx.ZaehlerSet.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<ZaehlerEntry>> Post(ClaimsPrincipal user, ZaehlerEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                if (entry.Wohnung != null)
                {
                    var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
                    var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                    if (!authRx.Succeeded)
                    {
                        return new ForbidResult();
                    }
                }
                else if (entry.Adresse != null)
                {
                    var adresse = GetAdresse(entry.Adresse, Ctx);

                    if (adresse != null)
                    {
                        var authRx = await Auth.AuthorizeAsync(user, adresse, [Operations.SubCreate]);
                        if (!authRx.Succeeded)
                        {
                            return new ForbidResult();
                        }
                    }

                }
                else
                {
                    return new BadRequestResult();
                }
                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        private async Task<ZaehlerEntry> Add(ZaehlerEntry entry)
        {
            var typ = (Zaehlertyp)entry.Typ.Id;
            var entity = new Zaehler(entry.Kennnummer, typ);

            await SetOptionalValues(entity, entry);
            Ctx.ZaehlerSet.Add(entity);
            Ctx.SaveChanges();

            return new ZaehlerEntry(entity, entry.Permissions);
        }
        public override async Task<ActionResult<ZaehlerEntry>> Put(ClaimsPrincipal user, int id, ZaehlerEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                // Ein geändertes Zähler-Ende verschiebt, welche Jahre den Zähler nutzen.
                if (entity.Ende != entry.Ende)
                {
                    var sperre = await AbrechnungsschutzService.SperreZaehlerAbJahr(
                        Ctx, id, AbrechnungsschutzService.FruehestesBetroffenesJahr(entity.Ende, entry.Ende));
                    if (sperre != null) return new ConflictObjectResult(sperre);
                }

                entity.Kennnummer = entry.Kennnummer;
                entity.Typ = (Zaehlertyp)entry.Typ.Id;

                await SetOptionalValues(entity, entry);
                Ctx.ZaehlerSet.Update(entity);
                Ctx.SaveChanges();

                return new ZaehlerEntry(entity, entry.Permissions);
            });
        }

        private async Task SetOptionalValues(Zaehler entity, ZaehlerEntry entry)
        {
            entity.Wohnung = entry.Wohnung is SelectionEntry w ? await Ctx.Wohnungen.FindAsync(w.Id) : null;

            entity.Adresse = entry.Adresse is AdresseEntryBase a ? GetAdresse(a, Ctx) : null;
            entity.Notiz = entry.Notiz;
            entity.Ende = entry.Ende;

            if (entry.SelectedUmlagen is IEnumerable<SelectionEntry> umlagen)
            {
                // Add missing umlagen
                entity.Umlagen.AddRange(umlagen
                    .Where(umlage => !entity.Umlagen.Exists(e => umlage.Id == e.UmlageId))
                    .SelectMany(w => Ctx.Umlagen.Where(u => u.UmlageId == w.Id)));
                // Remove old umlagen
                entity.Umlagen.RemoveAll(w => !umlagen.ToList().Exists(e => e.Id == w.UmlageId));
            }
        }
    }
}
