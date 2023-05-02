using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class SelectionListController : ControllerBase
    {
        public class SelectionEntry
        {
            public string Id { get; set; } = null!;
            public string Text { get; set; } = null!;
            public string? Filter { get; set; }

            public SelectionEntry() { }
            public SelectionEntry(Guid id, string text, string? filter = null)
            {
                Id = id.ToString();
                Text = text;
                Filter = filter;
            }

            public SelectionEntry(int id, string text, string? filter = null)
            {
                Id = id.ToString();
                Text = text;
                Filter = filter;
            }
        }

        private readonly ILogger<SelectionListController> _logger;
        private SaverwalterContext Ctx { get; }

        public SelectionListController(ILogger<SelectionListController> logger, SaverwalterContext ctx)
        {
            Ctx = ctx;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/selection/umlagen")]
        public IEnumerable<SelectionEntry> GetUmlagen()
        {
            return Ctx.Umlagen.ToList().Select(e =>
                new SelectionEntry(
                    e.UmlageId,
                    e.Wohnungen.ToList().GetWohnungenBezeichnung(),
                    ((int)e.Typ).ToString()))
                .ToList();
        }

        [HttpGet]
        [Route("api/selection/kontakte")]
        public IEnumerable<SelectionEntry> GetKontakte()
        {
            var nat = Ctx.NatuerlichePersonen
                .Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung, null))
                .ToList();
            var jur = Ctx.JuristischePersonen
                .Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung, null))
                .ToList();
            return nat.Concat(jur);
        }

        [HttpGet]
        [Route("api/selection/juristischepersonen")]
        public IEnumerable<SelectionEntry> GetJuristischePersonen()
        {
            return Ctx.JuristischePersonen
                .Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung, null))
                .ToList();
        }

        [HttpGet]
        [Route("api/selection/wohnungen")]
        public IActionResult GetWohnungen()
        {

            // Filter is used for Besitzer in Vertrag. TODO find a better name than filter.
            var entries = Ctx.Wohnungen
                .ToList()
                .Select(e =>
                    new SelectionEntry(
                            e.WohnungId,
                            $"{e.Adresse?.Anschrift ?? ""} - {e.Bezeichnung}",
                            e.BesitzerId.ToString()))
                .ToList();


            return new OkObjectResult(entries);
        }

        [HttpGet]
        [Route("api/selection/zaehler")]
        public IActionResult GetZaehler()
        {
            return new OkObjectResult(Ctx.ZaehlerSet
                .Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer, null))
                .ToList());
        }

        [HttpGet]
        [Route("api/selection/betriebskostentypen")]
        public IActionResult GetBetriebskostentypen()
        {
            return new OkObjectResult(Enum.GetValues(typeof(Betriebskostentyp))
                .Cast<Betriebskostentyp>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToDescriptionString()))
                .ToList());
        }

        [HttpGet]
        [Route("api/selection/umlageschluessel")]
        public IActionResult GetUmlageschluessel()
        {
            return new OkObjectResult(Enum.GetValues(typeof(Umlageschluessel))
                .Cast<Umlageschluessel>()
                .ToList()
                .Select(t => new SelectionEntry((int)t, t.ToDescriptionString()))
                .ToList());
        }

        [HttpGet]
        [Route("api/selection/hkvo_p9a2")]
        public IActionResult GetHKVO_P9A2()
        {
            return new OkObjectResult(Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList());
        }

        [HttpGet]
        [Route("api/selection/zaehlertypen")]
        public IActionResult GetZaehlertypen()
        {
            return new OkObjectResult(Enum.GetValues(typeof(Zaehlertyp))
                .Cast<Zaehlertyp>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList());
        }

        [HttpGet]
        [Route("api/selection/anreden")]
        public IActionResult GetAnreden()
        {
            return new OkObjectResult(Enum.GetValues(typeof(Anrede))
                .Cast<Anrede>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList());
        }
    }
}
