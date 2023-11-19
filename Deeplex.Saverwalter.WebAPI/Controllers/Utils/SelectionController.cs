using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class SelectionListController : ControllerBase
    {
        public class SelectionEntry
        {
            // string because UUID stuff
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
        [Route("api/selection/adressen")]
        public IActionResult GetAdressen()
        {
            var list = Ctx.Adressen.ToList()
                .Select(e =>
                    new SelectionEntry(
                        e.AdresseId,
                        e.Anschrift,
                        null))
                .ToList();
            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/betriebskostenrechnungen")]
        public IActionResult GetBetriebskostenrechnungen()
        {
            var list = Ctx.Betriebskostenrechnungen.ToList()
                .Select(e =>
                    new SelectionEntry(
                        e.BetriebskostenrechnungId,
                        $"{e.BetreffendesJahr} - {e.Umlage.Typ.Bezeichnung} - {e.Umlage.GetWohnungenBezeichnung()}",
                        null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/erhaltungsaufwendungen")]
        public IActionResult GetErhaltungsaufwendungen()
        {
            var list = Ctx.Erhaltungsaufwendungen.ToList()
                .Select(e =>
                    new SelectionEntry(
                        e.ErhaltungsaufwendungId,
                        $"{e.Bezeichnung} - {Ctx.FindPerson(e.AusstellerId).Bezeichnung}",
                        null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/mieten")]
        public IActionResult GetMieten()
        {
            var list = Ctx.Mieten.ToList()
                .Select(e =>
                    new SelectionEntry(
                    e.MieteId,
                    $"{e.Vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannt"} - {e.Vertrag.Wohnung.Bezeichnung} - {e.BetreffenderMonat.ToString("MM.yyyy")}",
                    null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/mietminderungen")]
        public IActionResult GetMietminderungen()
        {
            var list = Ctx.Mietminderungen.ToList()
                .Select(e =>
                    new SelectionEntry(
                    e.MietminderungId,
                    $"{e.Vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannt"} - {e.Vertrag.Wohnung.Bezeichnung} - {e.Beginn.ToString("MM.yyyy")}",
                    null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/umlagen")]
        public IActionResult GetUmlagen()
        {
            var list = Ctx.Umlagen.ToList()
                .Select(e =>
                    new SelectionEntry(
                        e.UmlageId,
                        $"{e.Typ.Bezeichnung} - {e.Wohnungen.ToList().GetWohnungenBezeichnung()}",
                        e.Typ.UmlagetypId.ToString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/umlagen_wohnungen")]
        public IActionResult GetUmlagenWohnungen()
        {
            var list = Ctx.Umlagen.ToList()
                .Select(e =>
                    new SelectionEntry(
                        e.UmlageId,
                        e.Wohnungen.ToList().GetWohnungenBezeichnung(),
                        e.Typ.UmlagetypId.ToString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/umlagen_verbrauch")]
        public IActionResult GetUmlagenVerbrauch()
        {
            var list = Ctx.Umlagen.ToList()
                .Where(e => e.Schluessel == Umlageschluessel.NachVerbrauch)
                .Select(e => new SelectionEntry(
                    e.UmlageId,
                    e.Typ.Bezeichnung + " - " + e.Wohnungen.ToList().GetWohnungenBezeichnung(),
                    null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/kontakte")]
        public IActionResult GetKontakte()
        {
            var nat = Ctx.NatuerlichePersonen
                .Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung, null))
                .ToList();
            var jur = Ctx.JuristischePersonen
                .Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung, null))
                .ToList();
            return new OkObjectResult(nat.Concat(jur));
        }

        [HttpGet]
        [Route("api/selection/natuerlichepersonen")]
        public IActionResult GetNatuerlichePersonen()
        {
            var list = Ctx.NatuerlichePersonen
                .Select(e => new SelectionEntry(e.NatuerlichePersonId, e.Bezeichnung, null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/juristischepersonen")]
        public IActionResult GetJuristischePersonen()
        {
            var list = Ctx.JuristischePersonen
                .Select(e => new SelectionEntry(e.JuristischePersonId, e.Bezeichnung, null))
                .ToList();

            return new OkObjectResult(list);
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
        [Route("api/selection/vertraege")]
        public IActionResult GetVertraege()
        {
            var list = Ctx.Vertraege
                .ToList()
                .Select(vertrag => new SelectionEntry(vertrag.VertragId, GetVertragName(vertrag, Ctx), null))
                .ToList();

            return new OkObjectResult(list);
        }

        private static string GetVertragName(Vertrag vertrag, SaverwalterContext ctx)
        {
            var wohnung = $"{vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {vertrag.Wohnung.Bezeichnung}";
            var mieterList = BetriebskostenabrechnungService.Utils
                .GetMieter(ctx, vertrag)
                .Select(person => person.Bezeichnung);
            var mieterText = string.Join(", ", mieterList);
            var vertragname = $"{wohnung} - {mieterText}";

            return vertragname;
        }

        [HttpGet]
        [Route("api/selection/zaehler")]
        public IActionResult GetZaehler()
        {
            var list = Ctx.ZaehlerSet
                .ToList()
                .Select(zaehler => new SelectionEntry(zaehler.ZaehlerId, getZaehlerName(zaehler), null))
                .ToList();

            return new OkObjectResult(list);
        }

        private static string getZaehlerName(Zaehler zaehler)
        {
            var adresse = "Keine Adresse";
            if (zaehler.Wohnung is Wohnung w && w.Adresse is Adresse wa)
            {
                adresse = wa.Strasse + " " + wa.Hausnummer + ", " + wa.Postleitzahl + " " + wa.Stadt;
            }
            else if (zaehler.Adresse is Adresse ad)
            {
                adresse = ad.Strasse + " " + ad.Hausnummer + ", " + ad.Postleitzahl + " " + ad.Stadt;
            }
            var wohnung = zaehler.Wohnung?.Bezeichnung ?? "Allgemeinzähler";
            return zaehler.Kennnummer + " - " + adresse + " - " + wohnung;
        }

        [HttpGet]
        [Route("api/selection/zaehlerstaende")]
        public IActionResult GetZaehlerStaende()
        {
            var list = Ctx.Zaehlerstaende
                .ToList()
                .Select(stand => new SelectionEntry(
                    stand.ZaehlerstandId,
                    $"{getZaehlerName(stand.Zaehler)} - {stand.Datum.ToString("dd.MM.yyyy")}",
                    null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/umlagetypen")]
        public IActionResult GetUmlagetypen()
        {
            var list = Ctx.Umlagetypen
                .ToList()
                .Select(typ => new SelectionEntry(typ.UmlagetypId, typ.Bezeichnung, null))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/umlageschluessel")]
        public IActionResult GetUmlageschluessel()
        {
            var list = Enum.GetValues(typeof(Umlageschluessel))
                .Cast<Umlageschluessel>()
                .ToList()
                .Select(t => new SelectionEntry((int)t, t.ToDescriptionString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/hkvo_p9a2")]
        public IActionResult GetHKVO_P9A2()
        {
            var list = Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToDescriptionString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/zaehlertypen")]
        public IActionResult GetZaehlertypen()
        {
            var list = Enum.GetValues(typeof(Zaehlertyp))
                .Cast<Zaehlertyp>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/anreden")]
        public IActionResult GetAnreden()
        {
            var list = Enum.GetValues(typeof(Anrede))
                .Cast<Anrede>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList();

            return new OkObjectResult(list);
        }
    }
}
