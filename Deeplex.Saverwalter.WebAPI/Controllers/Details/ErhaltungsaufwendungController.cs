using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/erhaltungsaufwendungen/{id}")]
    public class ErhaltungsaufwendungController
    {
        public class ErhaltungsaufwendungEntryBase
        {
            protected Erhaltungsaufwendung Entity { get; }

            public int Id => Entity.ErhaltungsaufwendungId;
            public double Betrag => Entity.Betrag;
            public string Datum => Entity.Datum.Datum();
            public string Notiz => Entity.Notiz ?? "";
            public string Bezeichnung => Entity.Bezeichnung;

            public ErhaltungsaufwendungEntryBase(Erhaltungsaufwendung entity)
            {
                Entity = entity;
            }
        }

        public class ErhaltungsaufwendungEntry : ErhaltungsaufwendungEntryBase
        {
            public PersonEntryBase Aussteller => new PersonEntryBase(Program.FindPerson(Entity.AusstellerId));

            public ErhaltungsaufwendungEntry(Erhaltungsaufwendung entity) : base(entity)
            {
            }
        }

        private readonly ILogger<ErhaltungsaufwendungController> _logger;

        public ErhaltungsaufwendungController(ILogger<ErhaltungsaufwendungController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetErhaltungsaufwendung")]
        public ErhaltungsaufwendungEntry Get(int id)
        {
            return new ErhaltungsaufwendungEntry(Program.ctx.Erhaltungsaufwendungen.Find(id));
        }
    }
}
