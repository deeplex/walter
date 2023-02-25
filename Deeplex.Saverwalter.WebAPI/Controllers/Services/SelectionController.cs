﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class SelectionListController : ControllerBase
    {
        public class SelectionListEntry
        {
            public string Id { get; }
            public string Text { get; }

            public SelectionListEntry(Guid id, string text)
            {
                Id = id.ToString();
                Text = text;
            }

            public SelectionListEntry(int id, string text)
            {
                Id = id.ToString();
                Text = text;
            }
        }

        private readonly ILogger<SelectionListController> _logger;
        private IWalterDbService DbService { get; }

        public SelectionListController(ILogger<SelectionListController> logger, IWalterDbService dbService)
        {
            DbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/selection/umlage")]
        public IEnumerable<SelectionListEntry> GetUmlagen()
        {
            return DbService.ctx.Umlagen.Select(e => new SelectionListEntry(e.UmlageId, e.Wohnungen.GetWohnungenBezeichnung())).ToList();
        }

        [HttpGet]
        [Route("api/selection/kontakte")]
        public IEnumerable<SelectionListEntry> GetKontakte()
        {
            var nat = DbService.ctx.NatuerlichePersonen.Select(e => new SelectionListEntry(e.PersonId, e.Bezeichnung)).ToList();
            var jur = DbService.ctx.JuristischePersonen.Select(e => new SelectionListEntry(e.PersonId, e.Bezeichnung)).ToList();
            return nat.Concat(jur);
        }

        [HttpGet]
        [Route("api/selection/wohnungen")]
        public IEnumerable<SelectionListEntry> GetWohnungen()
        {
            return DbService.ctx.Wohnungen.Select(e => new SelectionListEntry(e.WohnungId, e.Adresse.Anschrift + " - " + e.Bezeichnung)).ToList();
        }
    }
}
