using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class SelectionListController : ControllerBase
    {
        public class SelectionEntry
        {
            public string? Id { get; set; }
            public string? Text { get; set; }

            public SelectionEntry() { }
            public SelectionEntry(Guid id, string text)
            {
                Id = id.ToString();
                Text = text;
            }

            public SelectionEntry(int id, string text)
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
        public IEnumerable<SelectionEntry> GetUmlagen()
        {
            return DbService.ctx.Umlagen.Select(e => new SelectionEntry(e.UmlageId, e.Wohnungen.GetWohnungenBezeichnung())).ToList();
        }

        [HttpGet]
        [Route("api/selection/kontakte")]
        public IEnumerable<SelectionEntry> GetKontakte()
        {
            var nat = DbService.ctx.NatuerlichePersonen.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
            var jur = DbService.ctx.JuristischePersonen.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
            return nat.Concat(jur);
        }

        [HttpGet]
        [Route("api/selection/wohnungen")]
        public IEnumerable<SelectionEntry> GetWohnungen()
        {
            return DbService.ctx.Wohnungen.Select(e => new SelectionEntry(e.WohnungId, e.Adresse.Anschrift + " - " + e.Bezeichnung)).ToList();
        }
    }
}
