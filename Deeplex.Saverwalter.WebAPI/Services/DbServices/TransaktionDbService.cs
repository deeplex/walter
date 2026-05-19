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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class TransaktionDbService : WalterDbServiceBase<TransaktionEntry, Guid, Transaktion>
    {
        public TransaktionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public Task<PagedResult<TransaktionEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            TransaktionPermissionHandler.GetQueryable(Ctx, user).PagedAsync(query,
                searchPredicate: t => e =>
                    e.Verwendungszweck.ToLower().Contains(t) ||
                    (e.Zahler != null && (
                        (e.Zahler.Iban != null && e.Zahler.Iban.ToLower().Contains(t)) ||
                        (e.Zahler.Bank != null && e.Zahler.Bank.ToLower().Contains(t)))) ||
                    (e.Zahlungsempfaenger != null && (
                        (e.Zahlungsempfaenger.Iban != null && e.Zahlungsempfaenger.Iban.ToLower().Contains(t)) ||
                        (e.Zahlungsempfaenger.Bank != null && e.Zahlungsempfaenger.Bank.ToLower().Contains(t)))) ||
                    (e.Notiz != null && e.Notiz.ToLower().Contains(t)),
                applySort: (q, sortBy, dir) => sortBy switch
                {
                    "betrag" => q.SortBy(e => e.Betrag, dir),
                    "zahlungsdatum" => q.SortBy(e => e.Zahlungsdatum, dir),
                    "verwendungszweck" => q.SortBy(e => e.Verwendungszweck, dir),
                    _ => q.SortBy(e => e.Zahlungsdatum, "desc")
                },
                toEntry: async e => new TransaktionEntryBase(e, await GetPermissions(user, e, Auth)));

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

            if (entry.Zahler == null || await Ctx.Bankkontos.FindAsync(entry.Zahler.Id) == null)
            {
                return new BadRequestObjectResult($"Ungültiger Zahler");
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
            var zahler = entry.Zahler != null ? await Ctx.Bankkontos.FindAsync(entry.Zahler.Id) : null;
            if (zahler == null)
            {
                throw new ArgumentException($"Ungültiger Zahler");
            }

            var empfaenger = entry.Zahlungsempfaenger != null ? await Ctx.Bankkontos.FindAsync(entry.Zahlungsempfaenger.Id) : null;
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
                var zahler = entry.Zahler != null ? await Ctx.Bankkontos.FindAsync(entry.Zahler.Id) : null;
                if (zahler == null)
                {
                    throw new ArgumentException($"Ungültiger Zahler");
                }

                var empfaenger = entry.Zahlungsempfaenger != null ? await Ctx.Bankkontos.FindAsync(entry.Zahlungsempfaenger.Id) : null;
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
