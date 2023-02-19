﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Lists
{
    [ApiController]
    [Route("api/anhaenge")]
    public class AnhangListController : ControllerBase
    {
        public class AnhangListEntry
        {
            private Anhang Entity { get; }

            public Guid Id => Entity.AnhangId;
            public string FileName => Entity.FileName;
            public string CreationTime => Entity.CreationTime.Zeit();

            public AnhangListEntry(Anhang a)
            {
                Entity = a;
            }
        }

        private readonly ILogger<AnhangListController> _logger;

        public AnhangListController(ILogger<AnhangListController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetAnhaenge")]
        public IEnumerable<AnhangListEntry> Get()
        {
            return Program.ctx.Anhaenge.Select(e => new AnhangListEntry(e)).ToList();
        }
    }
}
