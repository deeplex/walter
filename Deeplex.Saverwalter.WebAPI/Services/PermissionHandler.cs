using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public static class Utils
    {
        public class Permissions(bool read)
        {
            public Permissions() : this(false) { }

            public bool Read { get; set; } = read;
            public bool Update { get; set; } = false;
            public bool Remove { get; set; } = false;
        }

        public static async Task<Permissions> GetPermissions<T>(ClaimsPrincipal user, T entity, IAuthorizationService auth)
        {
            var permissions = new Permissions();
            permissions.Read = (await auth.AuthorizeAsync(user, entity, [Operations.Read])).Succeeded;
            if (!permissions.Read)
            {
                return permissions;
            }
            permissions.Update = (await auth.AuthorizeAsync(user, entity, [Operations.Update])).Succeeded;
            if (!permissions.Update)
            {
                return permissions;
            }
            permissions.Remove = (await auth.AuthorizeAsync(user, entity, [Operations.Delete])).Succeeded;
            return permissions;
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
        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Adresse entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Wohnungen);
        }
    }

    public class BetriebskostenrechnungPermissionHandler : WohnungPermissionHandlerBase<Betriebskostenrechnung>
    {
        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Betriebskostenrechnung entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Umlage.Wohnungen);
        }
    }

    public class ErhaltungsaufwendungPermissionHandler : WohnungPermissionHandlerBase<Erhaltungsaufwendung>
    {
        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Erhaltungsaufwendung entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Wohnung);
        }
    }

    public class MietePermissionHandler : WohnungPermissionHandlerBase<Miete>
    {
        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           OperationAuthorizationRequirement requirement,
           Miete entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Vertrag.Wohnung);
        }
    }

    public class MietminderungPermissionHandler : WohnungPermissionHandlerBase<Mietminderung>
    {
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
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Umlage entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Wohnungen);
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
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Umlagetyp entity)
        {
            return HandleWohnungRequirementAsync(context, requirement, entity.Umlagen.SelectMany(umlage => umlage.Wohnungen));
        }
    }

    public class VertragPermissionHandler : WohnungPermissionHandlerBase<Vertrag>
    {
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
                if (requirement.Name == Operations.SubCreate.Name && (
                    IsAuthorized(userId, entity, VerwalterRolle.Vollmacht) ||
                    IsAuthorized(userId, entity, VerwalterRolle.Eigentuemer)))
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
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Zaehler entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Wohnung!);
        }
    }

    public class ZaehlerstandPermissionHandler : WohnungPermissionHandlerBase<Zaehlerstand>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Zaehlerstand entity)
        {
            return HandleWohnungSubRequirementAsync(context, requirement, entity.Zaehler.Wohnung!);
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
            foreach (var wohnung in entities)
            {
                HandleWohnungSubRequirementAsync(context, requirement, wohnung);
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
            else if (Guid.TryParse(context.User.FindAll(ClaimTypes.NameIdentifier)
                                .SingleOrDefault()?.Value,
                                out var userId))
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
                requirement.Name == Operations.Update.Name ||
                requirement.Name == Operations.Delete.Name
                ) && (
                IsAuthorized(userId, entity, VerwalterRolle.Vollmacht) ||
                IsAuthorized(userId, entity, VerwalterRolle.Eigentuemer)
                ))
            {
                context.Succeed(requirement);
            }
            else if (
                requirement.Name == Operations.Read.Name
                && (
                IsAuthorized(userId, entity, VerwalterRolle.Keine) ||
                IsAuthorized(userId, entity, VerwalterRolle.Vollmacht) ||
                IsAuthorized(userId, entity, VerwalterRolle.Eigentuemer)
                ))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        protected static bool MinAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung.Verwalter.Any(verwalter => verwalter.Rolle >= rolle && verwalter.UserAccount.Id == userId);

        protected static bool IsAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung.Verwalter.Any(verwalter => verwalter.Rolle == rolle && verwalter.UserAccount.Id == userId);
    }
}
