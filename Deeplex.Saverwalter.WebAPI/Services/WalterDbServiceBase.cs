using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public abstract class WalterDbServiceBase<T, U>
    {
        protected SaverwalterContext Ctx { get; }

        protected WalterDbServiceBase(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        protected readonly IAuthorizationService Auth;
        public abstract Task<ActionResult<U>> GetEntity(
            ClaimsPrincipal user,
            int id,
            OperationAuthorizationRequirement operation);

        protected async Task<ActionResult<U>> GetEntity(ClaimsPrincipal user, U? entity, OperationAuthorizationRequirement operation)
        {
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [operation]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            return entity;
        }

        protected async Task<ActionResult> HandleEntity(
            ClaimsPrincipal user,
            int id,
            OperationAuthorizationRequirement operation,
            Func<U, Task<ActionResult>> action)
        {
            var entity = await GetEntity(user, id, operation);

            if (entity.Result != null)
            {
                return entity.Result;
            }
            else if (entity.Value is U w)
            {
                return await action(w);
            }
            else
            {
                return new BadRequestResult();
            }
        }

        protected async Task<ActionResult<T>> HandleEntity(
            ClaimsPrincipal user,
            int id,
            OperationAuthorizationRequirement operation,
            Func<U, Task<ActionResult<T>>> action)
        {
            var entity = await GetEntity(user, id, operation);

            if (entity.Result != null)
            {
                return entity.Result;
            }
            else if (entity.Value is U w)
            {
                return await action(w);
            }
            else
            {
                return new BadRequestResult();
            }
        }

        public abstract Task<ActionResult<T>> Get(ClaimsPrincipal user, int id);
        public abstract Task<ActionResult<T>> Put(ClaimsPrincipal user, int id, T entry);
        public abstract Task<ActionResult<T>> Post(ClaimsPrincipal user, T entry);
        public abstract Task<ActionResult> Delete(ClaimsPrincipal user, int id);
    }
}
