using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.KontakteController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/kontakte/jur/{id}")]
    public class JuristischePersonController : ControllerBase
    {

        public sealed class JuristischePersonEntry : PersonEntry
        {
            private JuristischePerson Entity { get; }

            public JuristischePersonEntry(JuristischePerson entity) : base(entity)
            {
                Entity = entity;
            }
        }

        private readonly ILogger<JuristischePersonController> _logger;

        public JuristischePersonController(ILogger<JuristischePersonController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetJuristischePerson")]
        public JuristischePersonEntry Get(int id)
        {
            return new JuristischePersonEntry(Program.ctx.JuristischePersonen.Find(id));
        }
    }
}
