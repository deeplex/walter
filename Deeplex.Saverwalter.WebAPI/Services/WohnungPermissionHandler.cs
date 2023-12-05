using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;

namespace Deeplex.Saverwalter.WebAPI.Services
{
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

    public class WohnungPermissionHandler : AuthorizationHandler<OperationAuthorizationRequirement, Wohnung>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
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
                return HandleRequirementAsync(context, requirement, entity, userId);
            }
            return Task.CompletedTask;
        }

        protected Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Wohnung entity,
            Guid userId)
        {
            if ((
                IsAuthorized(userId, entity, VerwalterRolle.Vollmacht) ||
                IsAuthorized(userId, entity, VerwalterRolle.Eigentuemer)
                ) &&
                requirement.Name == Operations.Update.Name ||
                requirement.Name == Operations.SubCreate.Name ||
                requirement.Name == Operations.Delete.Name)
            {
                context.Succeed(requirement);
            }
            else if (IsAuthorized(userId, entity, VerwalterRolle.Keine) &&
                requirement.Name == Operations.Read.Name)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }


        protected static bool IsAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung.Verwalter.Any(verwalter => verwalter.Rolle == rolle && verwalter.UserAccount.Id == userId);
    }

    public abstract class WohnungPermissionHandlerBase<TEntity>
        : AuthorizationHandler<OperationAuthorizationRequirement, TEntity>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            TEntity entity)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
            else if (Guid.TryParse(context.User.FindAll(ClaimTypes.NameIdentifier)
                                .SingleOrDefault()?.Value,
                              out var userId))
            {
                return HandleRequirementAsync(context, requirement, entity, userId);
            }
            return Task.CompletedTask;
        }

        protected abstract Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            TEntity entity,
            Guid userId);

        protected static bool MinAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung.Verwalter.Any(verwalter => verwalter.Rolle >= rolle && verwalter.UserAccount.Id == userId);

        protected static bool IsAuthorized(Guid userId, Wohnung wohnung, VerwalterRolle rolle)
            => wohnung.Verwalter.Any(verwalter => verwalter.Rolle == rolle && verwalter.UserAccount.Id == userId);
    }
}
