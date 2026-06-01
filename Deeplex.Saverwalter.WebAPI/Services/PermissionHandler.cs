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

using System.Linq.Expressions;
using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public static class Utils
    {
        public static Expression<Func<Verwalter, bool>> HasRequiredAuth(VerwalterRolle rolle, Guid id)
        {
            return v =>
                v.UserAccount.Id == id &&
                (v.Rolle == VerwalterRolle.Eigentuemer ||
                (v.Rolle == VerwalterRolle.Vollmacht && (rolle == VerwalterRolle.Vollmacht || rolle == VerwalterRolle.Keine)) ||
                (v.Rolle == VerwalterRolle.Keine && rolle == VerwalterRolle.Keine));
        }

        public class Permissions(bool read)
        {
            public Permissions() : this(false) { }

            public bool Read { get; set; } = read;
            public bool Update { get; set; } = false;
            public bool Remove { get; set; } = false;
        }

        public static async Task<Permissions> GetPermissions<T>(ClaimsPrincipal user, T entity, IAuthorizationService auth)
        {
            var permissions = new Permissions
            {
                Read = await IsAuthorized(user, entity, auth, Operations.Read)
            };
            if (!permissions.Read)
            {
                return permissions;
            }
            permissions.Update = await IsAuthorized(user, entity, auth, Operations.Update);
            permissions.Remove = permissions.Update;

            return permissions;
        }

        private static async Task<bool> IsAuthorized<T>(ClaimsPrincipal user, T entity, IAuthorizationService auth, OperationAuthorizationRequirement operation)
        {
            if (auth is null)
            {
                return false;
            }

            var authTask = auth.AuthorizeAsync(user, entity, [operation]);
            if (authTask is null)
            {
                return false;
            }

            var result = await authTask;
            return result?.Succeeded == true;
        }

    }

    public static class Operations
    {
        public readonly static OperationAuthorizationRequirement SubCreate = new()
        {
            Name = nameof(SubCreate)
        };
        public readonly static OperationAuthorizationRequirement Read = new()
        {
            Name = nameof(Read)
        };
        public readonly static OperationAuthorizationRequirement Update = new()
        {
            Name = nameof(Update)
        };
        public readonly static OperationAuthorizationRequirement Delete = new()
        {
            Name = nameof(Delete)
        };
    }

    public class AdressePermissionHandler : WohnungPermissionHandlerBase<Adresse>
    {
        public static async Task<List<Adresse>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Adressen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Adresse>> GetEntriesForUser(Guid guid) => ctx.Adressen
                .Where(e => e.Wohnungen.Any(w =>
                    w.Verwalter.Count > 0 &&
                    w.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid))))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Adresse entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Wohnungen);
        }
    }

    public class AbrechnungsresultatPermissionHandler : WohnungPermissionHandlerBase<Abrechnungsresultat>
    {
        public static async Task<List<Abrechnungsresultat>> GetList(
            SaverwalterContext ctx,
            ClaimsPrincipal user,
            VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Abrechnungsresultate.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            async Task<List<Abrechnungsresultat>> GetEntriesForUser(Guid guid) =>
                await ctx.Abrechnungsresultate
                    .Where(e =>
                        e.Vertrag.Wohnung.Verwalter.Any() &&
                        e.Vertrag.Wohnung.Verwalter.AsQueryable()
                            .Any(Utils.HasRequiredAuth(rolle, guid)))
                    .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Abrechnungsresultat entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Vertrag.Wohnung);
        }
    }

    public class TransaktionPermissionHandler(SaverwalterContext ctx)
        : AuthorizationHandler<OperationAuthorizationRequirement, Transaktion>
    {
        /// <summary>
        /// Buchungskonto-IDs, die zu einer Wohnung gehören, die der Nutzer in der
        /// gegebenen Rolle verwaltet — inklusive der Konten der zugehörigen Verträge,
        /// Garagenverträge und Umlagen. Über diese Konten werden Transaktionen einer
        /// Wohnung zugeordnet (eine Transaktion hat keine direkte Wohnungs-Beziehung).
        /// </summary>
        public static IQueryable<int> ManagedBuchungskontoIds(
            SaverwalterContext ctx, Guid guid, VerwalterRolle rolle)
        {
            var wohnungIds = ctx.Wohnungen
                .Where(w => w.Verwalter.Count > 0 &&
                    w.Verwalter.AsQueryable().Any(Utils.HasRequiredAuth(rolle, guid)))
                .Select(w => w.WohnungId);

            var wohnungen = ctx.Wohnungen.Where(w => wohnungIds.Contains(w.WohnungId));
            var vertraege = ctx.Vertraege.Where(v => wohnungIds.Contains(v.Wohnung.WohnungId));
            var garageVertraege = ctx.GarageVertraege
                .Where(gv => gv.Vertrag != null && wohnungIds.Contains(gv.Vertrag.Wohnung.WohnungId));
            var umlagen = ctx.Umlagen.Where(u => u.Wohnungen.Any(w => wohnungIds.Contains(w.WohnungId)));

            return wohnungen.Select(w => w.MietErtragskonto.BuchungskontoId)
                .Union(wohnungen.Select(w => w.AufwandsKonto.BuchungskontoId))
                .Union(vertraege.Select(v => v.MietBuchungskonto.BuchungskontoId))
                .Union(vertraege.Select(v => v.NkBuchungskonto.BuchungskontoId))
                .Union(vertraege.Select(v => v.BkAbrechnungsKonto.BuchungskontoId))
                .Union(vertraege.Select(v => v.ZahlungsKonto.BuchungskontoId))
                .Union(vertraege.Select(v => v.MietminderungsKonto.BuchungskontoId))
                .Union(garageVertraege.Select(gv => gv.MietBuchungskonto.BuchungskontoId))
                .Union(garageVertraege.Select(gv => gv.ZahlungsKonto.BuchungskontoId))
                .Union(garageVertraege.Select(gv => gv.Garage.Ertragskonto.BuchungskontoId))
                .Union(umlagen.Select(u => u.NkVerrechnungsKonto.BuchungskontoId))
                .Union(umlagen.Select(u => u.ZahlungsKonto.BuchungskontoId));
        }

        public static IQueryable<Transaktion> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
            {
                return ctx.Transaktionen;
            }

            var kontoIds = ManagedBuchungskontoIds(ctx, user.GetUserId(), VerwalterRolle.Keine);
            return ctx.Transaktionen
                .Where(t => t.Buchungssaetze.Any(s =>
                    s.Buchungszeilen.Any(z => kontoIds.Contains(z.Buchungskonto.BuchungskontoId))));
        }

        protected override async Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Transaktion entity)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Lesen darf jeder Verwalter der betroffenen Wohnung (Rolle Keine genügt),
            // Ändern/Löschen/Buchen erfordert Vollmacht — analog zur Wohnung.
            var rolle = requirement.Name == Operations.Read.Name
                ? VerwalterRolle.Keine
                : VerwalterRolle.Vollmacht;

            var kontoIds = ManagedBuchungskontoIds(ctx, context.User.GetUserId(), rolle);
            var darfZugreifen = await ctx.Transaktionen
                .Where(t => t.TransaktionId == entity.TransaktionId)
                .AnyAsync(t => t.Buchungssaetze.Any(s =>
                    s.Buchungszeilen.Any(z => kontoIds.Contains(z.Buchungskonto.BuchungskontoId))));

            if (darfZugreifen)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class KontaktPermissionHandler : WohnungPermissionHandlerBase<Kontakt>
    {
        public static IQueryable<Kontakt> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            return ctx.Kontakte;
        }

        public static async Task<List<Kontakt>> GetList(
            SaverwalterContext ctx,
            ClaimsPrincipal user,
            VerwalterRolle _)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Kontakte.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Kontakt>> GetEntriesForUser(Guid _) => ctx.Kontakte.ToListAsync();
        }

        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Kontakt entity)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class MietminderungPermissionHandler : WohnungPermissionHandlerBase<Mietminderung>
    {
        public static async Task<List<Mietminderung>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Mietminderungen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Mietminderung>> GetEntriesForUser(Guid guid) => ctx.Mietminderungen
                .Where(e =>
                    e.Vertrag.Wohnung.Verwalter.Count != 0 &&
                    e.Vertrag.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Mietminderung entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Vertrag.Wohnung);
        }
    }

    public class UmlagePermissionHandler : WohnungPermissionHandlerBase<Umlage>
    {
        public static IQueryable<Umlage> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return ctx.Umlagen;
            var guid = user.GetUserId();
            return ctx.Umlagen
                .Where(e => e.Wohnungen.Any(w =>
                    w.Verwalter.Count != 0 &&
                    w.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid))));
        }

        public static async Task<List<Umlage>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Umlagen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Umlage>> GetEntriesForUser(Guid guid) => ctx.Umlagen
                .Where(e =>
                    e.Wohnungen.Any(w => w.Verwalter.Count != 0 &&
                    w.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid))))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Umlage entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Wohnungen);
        }
    }

    public class WohnungenPermissionHandler : WohnungPermissionHandlerBase<IEnumerable<Wohnung>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IEnumerable<Wohnung> entities)
        {
            return HandleWohnungRequirementAsync(context, requirement, entities);
        }
    }

    public class UmlagenPermissionHandler : WohnungPermissionHandlerBase<IEnumerable<Umlage>>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            IEnumerable<Umlage> entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.SelectMany(e => e.Wohnungen));
        }
    }

    public class UmlagetypPermissionHandler : WohnungPermissionHandlerBase<Umlagetyp>
    {
        public static async Task<List<Umlagetyp>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Umlagetypen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Umlagetyp>> GetEntriesForUser(Guid guid) => ctx.Umlagetypen
                .Where(e => e.Umlagen.Count == 0 ||
                    e.Umlagen.Any(u => u.Wohnungen
                        .Any(w =>
                            w.Verwalter.Count != 0 &&
                            w.Verwalter.AsQueryable()
                                .Any(Utils.HasRequiredAuth(rolle, guid)))))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Umlagetyp entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Umlagen.SelectMany(umlage => umlage.Wohnungen));
        }
    }

    public class GaragePermissionHandler : WohnungPermissionHandlerBase<Garage>
    {
        public static IQueryable<Garage> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            // Garagen sind allen verwaltenden Nutzern sichtbar
            if (user.IsInRole("Admin"))
                return ctx.Garagen;
            var guid = user.GetUserId();
            return ctx.Garagen
                .Where(e => e.Vertraege.Any(gv =>
                    gv.Vertrag != null &&
                    gv.Vertrag.Wohnung.Verwalter.Count > 0 &&
                    gv.Vertrag.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid)))
                    || ctx.Wohnungen.Any(w =>
                        w.Verwalter.Count > 0 &&
                        w.Verwalter.AsQueryable()
                            .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid))));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Garage entity)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class GarageVertragPermissionHandler : WohnungPermissionHandlerBase<GarageVertrag>
    {
        public static IQueryable<GarageVertrag> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return ctx.GarageVertraege;
            var guid = user.GetUserId();
            return ctx.GarageVertraege
                .Where(e =>
                    (e.Vertrag != null &&
                     e.Vertrag.Wohnung.Verwalter.Count > 0 &&
                     e.Vertrag.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid))) ||
                    ctx.Wohnungen.Any(w =>
                        w.Verwalter.Count > 0 &&
                        w.Verwalter.AsQueryable()
                            .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid))));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            GarageVertrag entity)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class VertragPermissionHandler : WohnungPermissionHandlerBase<Vertrag>
    {
        public static IQueryable<Vertrag> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return ctx.Vertraege;
            var guid = user.GetUserId();
            return ctx.Vertraege
                .Where(e =>
                    e.Wohnung.Verwalter.Count > 0 &&
                    e.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid)));
        }

        public static async Task<List<Vertrag>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Vertraege.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Vertrag>> GetEntriesForUser(Guid guid) => ctx.Vertraege
                .Where(e =>
                    e.Wohnung.Verwalter.Count > 0 &&
                    e.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Vertrag entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Wohnung);
        }
    }

    public class VertragVersionPermissionHandler : WohnungPermissionHandlerBase<VertragVersion>
    {
        public static async Task<List<VertragVersion>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.VertragVersionen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<VertragVersion>> GetEntriesForUser(Guid guid) => ctx.VertragVersionen
                .Where(e => e.Vertrag.Wohnung.Verwalter.Count > 0 && e.Vertrag.Wohnung.Verwalter.AsQueryable().Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            VertragVersion entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Vertrag.Wohnung);
        }
    }

    public class WohnungPermissionHandler : WohnungPermissionHandlerBase<Wohnung>
    {
        public static IQueryable<Wohnung> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return ctx.Wohnungen;
            var guid = user.GetUserId();
            return ctx.Wohnungen
                .Where(e =>
                    e.Verwalter.Count > 0 &&
                    e.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid)));
        }

        public static async Task<List<Wohnung>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Wohnungen.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Wohnung>> GetEntriesForUser(Guid guid) => ctx.Wohnungen
                .Where(e =>
                    e.Verwalter.Count > 0 &&
                    e.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Wohnung entity)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
            else if (Guid.TryParse(context.User.FindAll(ClaimTypes.NameIdentifier)
                                .SingleOrDefault()?.Value,
                                out var userId))
            {
                if (requirement.Name == Operations.SubCreate.Name &&
                    IsAuthorized(userId, entity, VerwalterRolle.Vollmacht))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    return HandleWohnungSubRequirementAsync(context, requirement, entity, userId);
                }
            }
            return Task.CompletedTask;
        }
    }

    public class ZaehlerPermissionHandler : WohnungPermissionHandlerBase<Zaehler>
    {
        public static IQueryable<Zaehler> GetQueryable(SaverwalterContext ctx, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin"))
                return ctx.ZaehlerSet;
            var guid = user.GetUserId();
            return ctx.ZaehlerSet
                .Where(e =>
                    e.Wohnung == null ||
                    e.Wohnung.Verwalter.Count > 0 &&
                    e.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(VerwalterRolle.Keine, guid)));
        }

        public static async Task<List<Zaehler>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.ZaehlerSet.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Zaehler>> GetEntriesForUser(Guid guid) => ctx.ZaehlerSet
                // TODO e.Wohnung is null is obviously stupid. Fix this.
                .Where(e =>
                    e.Wohnung == null ||
                    e.Wohnung.Verwalter.Count > 0 &&
                    e.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Zaehler entity)
        {
            if (entity.Wohnung is not null)
            {
                return HandleWohnungSubRequirementAsync(context, requirement, entity.Wohnung);
            }
            else
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
    }

    public class ZaehlerstandPermissionHandler : WohnungPermissionHandlerBase<Zaehlerstand>
    {
        public static async Task<List<Zaehlerstand>> GetList(SaverwalterContext ctx, ClaimsPrincipal user, VerwalterRolle rolle)
        {
            return await (user.IsInRole("Admin")
                ? ctx.Zaehlerstaende.ToListAsync()
                : GetEntriesForUser(user.GetUserId()));

            Task<List<Zaehlerstand>> GetEntriesForUser(Guid guid) => ctx.Zaehlerstaende
                .Where(e =>
                    e.Zaehler.Wohnung == null ||
                    e.Zaehler.Wohnung.Verwalter.Count > 0 &&
                    e.Zaehler.Wohnung.Verwalter.AsQueryable()
                        .Any(Utils.HasRequiredAuth(rolle, guid)))
                .ToListAsync();
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Zaehlerstand entity)
        {
            if (entity.Zaehler.Wohnung != null)
            {
                return HandleWohnungSubRequirementAsync(context, requirement, entity.Zaehler.Wohnung);
            }
            if (entity.Zaehler.Adresse == null || entity.Zaehler.Adresse.Wohnungen.Count == 0)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            else
            {
                foreach (var wohnung in entity.Zaehler.Adresse.Wohnungen.ToList())
                {
                    HandleWohnungSubRequirementAsync(context, requirement, wohnung);
                }

                return Task.CompletedTask;
            }
        }
    }

    public abstract class WohnungPermissionHandlerBase<TEntity>
        : AuthorizationHandler<OperationAuthorizationRequirement, TEntity>
    {
        protected abstract override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            TEntity entity);

        protected Task HandleWohnungRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            IEnumerable<Wohnung> entities)
        {
            if (entities.Count() == 0)
            {
                context.Succeed(requirement);
            }

            foreach (var wohnung in entities)
            {
                HandleWohnungSubRequirementAsync(context, requirement, wohnung);
                if (context.HasSucceeded)
                {
                    break;
                }
            }

            return Task.CompletedTask;

        }

        protected Task HandleWohnungSubRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Wohnung entity)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
            else if (context.User.GetUserId() is Guid userId)
            {
                return HandleWohnungSubRequirementAsync(context, requirement, entity, userId);
            }
            return Task.CompletedTask;
        }

        protected Task HandleWohnungSubRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Wohnung entity,
            Guid userId)
        {
            if ((
                requirement.Name == Operations.SubCreate.Name ||
                requirement.Name == Operations.Update.Name ||
                requirement.Name == Operations.Delete.Name
                ) && IsAuthorized(userId, entity, VerwalterRolle.Vollmacht))
            {
                context.Succeed(requirement);
            }
            else if (
                requirement.Name == Operations.Read.Name
                && IsAuthorized(userId, entity, VerwalterRolle.Keine))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        protected static bool IsAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung != null &&
               wohnung.Verwalter.Count > 0 &&
               wohnung.Verwalter.AsQueryable()
                .Any(Utils.HasRequiredAuth(rolle, userId));
    }

    public class WohnungVersionPermissionHandler : WohnungPermissionHandlerBase<WohnungVersion>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            WohnungVersion entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Wohnung);
        }
    }

    public class UmlageVersionPermissionHandler : WohnungPermissionHandlerBase<UmlageVersion>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            UmlageVersion entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Umlage.Wohnungen);
        }
    }
}
