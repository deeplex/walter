using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public interface IControllerService<T>
    {
        public IActionResult Get(int id);
        public IActionResult Put(int id, T entry);
        public IActionResult Post(T entry);
        public IActionResult Delete(int id, T entry);
    }
}
