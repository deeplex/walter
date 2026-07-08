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
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.BuchungssaetzeController;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    /// <summary>
    /// Trägt die Sichtbarkeitsregeln der Buchungssätze für die Datei-Endpunkte
    /// (FileControllerBase) — die eigentlichen CRUD-Routen implementiert der
    /// BuchungssaetzeController selbst.
    /// </summary>
    public class BuchungssatzDbService : WalterDbServiceBase<BuchungssatzEntry, Guid, Buchungssatz>
    {
        public BuchungssatzDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
            : base(ctx, authorizationService)
        {
        }

        /// <summary>
        /// Buchungssätze, die der Nutzer sehen darf — Admin alle, sonst nur Sätze
        /// mit mindestens einer Zeile auf einem Konto einer verwalteten Wohnung.
        /// </summary>
        public static IQueryable<Buchungssatz> Scoped(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
            {
                return ctx.Buchungssaetze;
            }

            var kontoIds = TransaktionPermissionHandler
                .ManagedBuchungskontoIds(ctx, user.GetUserId(), VerwalterRolle.Keine);
            return ctx.Buchungssaetze.Where(s =>
                s.Buchungszeilen.Any(z => kontoIds.Contains(z.Buchungskonto.BuchungskontoId)));
        }

        public override async Task<ActionResult<Buchungssatz>> GetEntity(
            ClaimsPrincipal user, Guid id, OperationAuthorizationRequirement operation)
        {
            var satz = await Scoped(Ctx, user).FirstOrDefaultAsync(s => s.BuchungssatzId == id);
            if (satz != null)
            {
                return satz;
            }

            // Existiert der Satz, ist aber außerhalb des Sichtbereichs? -> 403 statt 404
            return await Ctx.Buchungssaetze.AnyAsync(s => s.BuchungssatzId == id)
                ? new ForbidResult()
                : new NotFoundResult();
        }

        // CRUD läuft über die eigenen Routen des BuchungssaetzeController.
        public override Task<ActionResult<BuchungssatzEntry>> Get(ClaimsPrincipal user, Guid id) =>
            Task.FromResult<ActionResult<BuchungssatzEntry>>(new StatusCodeResult(405));
        public override Task<ActionResult<BuchungssatzEntry>> Put(ClaimsPrincipal user, Guid id, BuchungssatzEntry entry) =>
            Task.FromResult<ActionResult<BuchungssatzEntry>>(new StatusCodeResult(405));
        public override Task<ActionResult> Delete(ClaimsPrincipal user, Guid id) =>
            Task.FromResult<ActionResult>(new StatusCodeResult(405));
    }
}
