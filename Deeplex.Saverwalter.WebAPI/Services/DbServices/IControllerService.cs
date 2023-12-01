using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public interface IControllerService<T>
    {
        public IActionResult Get(int id);
        public IActionResult Put(int id, T entry);
        public IActionResult Post(T entry);
        public IActionResult Delete(int id);
    }

    public interface ICRUDService<T>
    {
        public Task<IActionResult> Get(ClaimsPrincipal user, int id);
        public Task<IActionResult> Put(ClaimsPrincipal user, int id, T entry);
        public Task<IActionResult> Post(ClaimsPrincipal user, T entry);
        public Task<IActionResult> Delete(ClaimsPrincipal user, int id);
    }
}
