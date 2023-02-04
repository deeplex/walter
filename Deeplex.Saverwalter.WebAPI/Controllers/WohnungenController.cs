﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WohnungenController : ControllerBase
    {
        public class WohnungListEntry
        {
            private Wohnung Entity { get; }

            public int id => Entity.WohnungId;
            public string Bezeichnung => Entity.Bezeichnung;
            public string Besitzer => "TODO";
            public string Bewohner => "TODO";
            public string Anschrift => "TODO";

            public WohnungListEntry(Wohnung w)
            {
                Entity = w;
            }
        }

        private readonly ILogger<WohnungenController> _logger;

        public WohnungenController(ILogger<WohnungenController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWohnungen")]
        public IEnumerable<WohnungListEntry> Get()
        {
            return Program.ctx.Wohnungen.Select(e => new WohnungListEntry(e)).ToList();
        }
    }
}
