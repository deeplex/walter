﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.WohnungController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Details
{
    [ApiController]
    [Route("api/umlagen/{id}")]
    public class UmlageController
    {
        public class UmlageEntryBase
        {
            protected Umlage Entity { get; }

            public int Id => Entity.UmlageId;
            public string Notiz => Entity.Notiz ?? "";
            public string WohnungenBezeichnung => Entity.GetWohnungenBezeichnung();
            public Betriebskostentyp Typ => Entity.Typ;

            public UmlageEntryBase(Umlage entity)
            {
                Entity = entity;
            }
        }

        public class UmlageEntry : UmlageEntryBase
        {
            public IEnumerable<WohnungEntryBase> Wohnungen => Entity.Wohnungen.Select(e => new WohnungEntryBase(e));
            public IEnumerable<AnhangEntry> Anhaenge => Entity.Anhaenge.Select(e => new AnhangEntry(e));

            public UmlageEntry(Umlage entity) : base(entity)
            {
            }
        }

        private readonly ILogger<UmlageController> _logger;

        public UmlageController(ILogger<UmlageController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetUmlage")]
        public UmlageEntry Get(int id)
        {
            return new UmlageEntry(Program.ctx.Umlagen.Find(id));
        }
    }
}
