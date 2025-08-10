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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class TransaktionDbService : WalterDbServiceBase<TransaktionEntry, Guid, Transaktion>
    {
        public TransaktionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<TransaktionEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await TransaktionPermissionHandler.GetList(Ctx, user);

            return await Task.WhenAll(list
                .Select(async e => new TransaktionEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Transaktion>> GetEntity(ClaimsPrincipal user, Guid id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Transaktionen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<TransaktionEntry>> Get(ClaimsPrincipal user, Guid id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new TransaktionEntry(entity, permissions);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                Ctx.Transaktionen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<TransaktionEntry>> Post(ClaimsPrincipal user, TransaktionEntry entry)
        {
            if (entry.Id != Guid.Empty)
            {
                return new BadRequestResult();
            }

            var zahler = (await Ctx.Kontakte.FindAsync(entry.Zahler.Id))!;
            if (zahler == null)
            {
                return new BadRequestObjectResult($"Ungültiger Zahler");
            }
            var authRx = await Auth.AuthorizeAsync(user, zahler, [Operations.SubCreate]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                return await Add(entry);
            }
            catch (Exception)
            {
                return new BadRequestObjectResult($"Fehler beim Hinzufügen der Transaktion.");
            }
        }

        private async Task<TransaktionEntry> Add(TransaktionEntry entry)
        {
            var zahler = (await Ctx.Kontakte.FindAsync(entry.Zahler.Id))!;
            if (zahler == null)
            {
                throw new ArgumentException($"Ungültiger Zahler");
            }

            var empfaenger = (await Ctx.Kontakte.FindAsync(entry.Zahlungsempfaenger.Id))!;
            if (empfaenger == null)
            {
                throw new ArgumentException($"Ungültiger Zahlungsempfänger");
            }

            var entity = new Transaktion()
            {
                Zahler = zahler,
                Zahlungsempfaenger = empfaenger,
                Zahlungsdatum = entry.Zahlungsdatum,
                Betrag = entry.Betrag,
                Verwendungszweck = entry.Verwendungszweck
            };

            SetOptionalValues(entity, entry);
            Ctx.Transaktionen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new TransaktionEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<TransaktionEntry>> Put(
            ClaimsPrincipal user,
            Guid id, TransaktionEntry entry)
        {
            return await HandleEntity(user, id, Operations.Update, async (entity) =>
            {
                entity.Betrag = entry.Betrag;
                var zahler = (await Ctx.Kontakte.FindAsync(entry.Zahler.Id))!;
                if (zahler == null)
                {
                    throw new ArgumentException($"Ungültiger Zahler");
                }

                var empfaenger = (await Ctx.Kontakte.FindAsync(entry.Zahlungsempfaenger.Id))!;
                if (empfaenger == null)
                {
                    throw new ArgumentException($"Ungültiger Zahlungsempfänger");
                }

                entity.Zahler = zahler;
                entity.Zahlungsempfaenger = empfaenger;
                entity.Zahlungsdatum = entry.Zahlungsdatum;
                entity.Verwendungszweck = entry.Verwendungszweck;
                entity.Betrag = entry.Betrag;
                SetOptionalValues(entity, entry);

                Ctx.Transaktionen.Update(entity);
                await Ctx.SaveChangesAsync();

                return new TransaktionEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(Transaktion entity, TransaktionEntry entry)
        {
            if (entity.TransaktionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
