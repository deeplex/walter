using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Xml.Linq;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/wohnungen/{id}")]
    public class WohnungController
    {
        public class WohnungEntryBase
        {
            private Wohnung Entity { get; }

            public int id => Entity.WohnungId;
            public string Bezeichnung => Entity.Bezeichnung;
            public double Wohnflaeche => Entity.Wohnflaeche;
            public double Nutzflaeche => Entity.Nutzflaeche;
            public int Einheiten => Entity.Nutzeinheit;
            public string Notiz => Entity.Notiz ?? "";
            public AdresseEntry? Adresse => Entity.Adresse is Adresse a ? new AdresseEntry(a) : null;
            public string Anschrift => Entity.Adresse.Anschrift + ", " + Entity.Bezeichnung;

            public WohnungEntryBase(Wohnung entity)
            {
                Entity = entity;
            }
        }

        public class WohnungEntry : WohnungEntryBase
        {
            // Wohnungen die an der selben Adresse sind.
            // Zähler
            // Betriebskostenrechnungen
            // Erhaltungsaufwendungen
            // Umlagen

            public WohnungEntry(Wohnung entity) : base(entity) { }
        }

        private readonly ILogger<WohnungController> _logger;

        public WohnungController(ILogger<WohnungController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWohnung")]
        public WohnungEntry Get(int id)
        {
            return new WohnungEntry(Program.ctx.Wohnungen.Find(id));
        }
    }
}