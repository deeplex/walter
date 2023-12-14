using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public interface ICRUDService<T>
    {
        public Task<ActionResult<T>> Get(ClaimsPrincipal user, int id);
        public Task<ActionResult<T>> Put(ClaimsPrincipal user, int id, T entry);
        public Task<ActionResult<T>> Post(ClaimsPrincipal user, T entry);
        public Task<ActionResult> Delete(ClaimsPrincipal user, int id);
    }

    public interface ICRUDServiceGuid<T>
    {
        public Task<ActionResult<T>> Get(ClaimsPrincipal user, Guid id);
        public Task<ActionResult<T>> Put(ClaimsPrincipal user, Guid id, T entry);
        public Task<ActionResult<T>> Post(ClaimsPrincipal user, T entry);
        public Task<ActionResult> Delete(ClaimsPrincipal user, Guid id);
    }
}
