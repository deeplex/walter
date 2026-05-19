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
using static Deeplex.Saverwalter.WebAPI.Controllers.BankkontoController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.TransaktionController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class BankkontoDbService : WalterDbServiceBase<BankkontoEntry, int, Bankkonto>
    {
        public BankkontoDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        private static Permissions AllowAll => new() { Read = true, Update = true, Remove = true };

        public Task<PagedResult<BankkontoEntryBase>> GetList(ClaimsPrincipal user, PagedQuery query) =>
            Ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .Include(b => b.Besitzer)
                .AsQueryable()
                .PagedAsync(query,
                    searchPredicate: t => e =>
                        (e.Iban != null && e.Iban.ToLower().Contains(t)) ||
                        (e.Bank != null && e.Bank.ToLower().Contains(t)) ||
                        e.Besitzer.Any(k => k.Name.ToLower().Contains(t)),
                    applySort: (q, sortBy, dir) => sortBy switch
                    {
                        "bank" => q.SortBy(e => e.Bank, dir),
                        "iban" => q.SortBy(e => e.Iban, dir),
                        _ => q.SortBy(e => e.Iban, dir)
                    },
                    toEntry: e => Task.FromResult(new BankkontoEntryBase(e, AllowAll)));

        public override async Task<ActionResult<Bankkonto>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Bankkontos.FindAsync(id);
            return entity is null ? new NotFoundResult() : entity;
        }

        public override async Task<ActionResult<BankkontoEntry>> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .Include(b => b.Besitzer)
                .FirstOrDefaultAsync(b => b.BankkontoId == id);

            if (entity is null) return new NotFoundResult();

            var entry = new BankkontoEntry(entity, AllowAll);
            entry.Transaktionen = Ctx.Transaktionen
                .Where(t => t.Zahler != null && t.Zahler.BankkontoId == id
                    || t.Zahlungsempfaenger != null && t.Zahlungsempfaenger.BankkontoId == id)
                .Include(t => t.Zahler)
                .Include(t => t.Zahlungsempfaenger)
                .AsEnumerable()
                .Select(t => new TransaktionEntryBase(t, AllowAll))
                .ToList();

            return entry;
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Bankkontos.FindAsync(id);
            if (entity is null) return new NotFoundResult();

            Ctx.Bankkontos.Remove(entity);
            await Ctx.SaveChangesAsync();
            return new OkResult();
        }

        public override async Task<ActionResult<BankkontoEntry>> Post(ClaimsPrincipal user, BankkontoEntry entry)
        {
            if (entry.Id != 0) return new BadRequestResult();

            try
            {
                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<BankkontoEntry> Add(BankkontoEntry entry)
        {
            var bezeichnung = entry.Iban ?? entry.Bank ?? "Bankkonto";
            var entity = new Bankkonto
            {
                BuchungsKonto = new Buchungskonto(
                    $"BK-{Guid.NewGuid():N}",
                    bezeichnung,
                    BuchungskontoTyp.Aktiv)
            };

            SetOptionalValues(entity, entry);
            Ctx.Bankkontos.Add(entity);
            await Ctx.SaveChangesAsync();

            return new BankkontoEntry(entity, AllowAll);
        }

        public override async Task<ActionResult<BankkontoEntry>> Put(ClaimsPrincipal user, int id, BankkontoEntry entry)
        {
            var entity = await Ctx.Bankkontos
                .Include(b => b.BuchungsKonto)
                .Include(b => b.Besitzer)
                .FirstOrDefaultAsync(b => b.BankkontoId == id);

            if (entity is null) return new NotFoundResult();

            entity.BuchungsKonto.Bezeichnung = entry.Iban ?? entry.Bank ?? entity.BuchungsKonto.Bezeichnung;

            SetOptionalValues(entity, entry);
            Ctx.Bankkontos.Update(entity);
            await Ctx.SaveChangesAsync();

            return new BankkontoEntry(entity, AllowAll);
        }

        private void SetOptionalValues(Bankkonto entity, BankkontoEntry entry)
        {
            entity.Bank = entry.Bank;
            entity.Iban = entry.Iban;
            entity.Notiz = entry.Notiz;

            if (entry.SelectedBesitzer is IEnumerable<SelectionEntry> besitzer)
            {
                var besitzerList = besitzer.ToList();
                entity.Besitzer.RemoveAll(k => !besitzerList.Exists(e => e.Id == k.KontaktId));
                entity.Besitzer.AddRange(besitzerList
                    .Where(e => !entity.Besitzer.Exists(k => k.KontaktId == e.Id))
                    .SelectMany(e => Ctx.Kontakte.Where(k => k.KontaktId == e.Id)));
            }
        }
    }
}
