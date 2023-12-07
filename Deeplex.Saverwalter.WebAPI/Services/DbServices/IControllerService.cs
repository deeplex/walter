using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public interface ICRUDService<T>
    {
        public Task<IActionResult> Get(ClaimsPrincipal user, int id);
        public Task<IActionResult> Put(ClaimsPrincipal user, int id, T entry);
        public Task<IActionResult> Post(ClaimsPrincipal user, T entry);
        public Task<IActionResult> Delete(ClaimsPrincipal user, int id);
    }

    public interface ICRUDServiceGuid<T>
    {
        public Task<IActionResult> Get(ClaimsPrincipal user, Guid id);
        public Task<IActionResult> Put(ClaimsPrincipal user, Guid id, T entry);
        public Task<IActionResult> Post(ClaimsPrincipal user, T entry);
        public Task<IActionResult> Delete(ClaimsPrincipal user, Guid id);
    }
}
