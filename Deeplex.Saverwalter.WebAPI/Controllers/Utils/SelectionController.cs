using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Services
{
    public class SelectionListController : ControllerBase
    {
        public class SelectionEntry
        {
            public int Id { get; set; }
            public string Text { get; set; } = null!;
            public string? Filter { get; set; }

            public SelectionEntry() { }

            public SelectionEntry(int id, string text, string? filter = null)
            {
                Id = id;
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
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetAdressen()
        {
            var list = await AdressePermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);
            return Ok(list.Select(e => new SelectionEntry(e.AdresseId, e.Anschrift)));
        }

        [HttpGet]
        [Route("api/selection/verwalterrollen")]
        public ActionResult<IEnumerable<SelectionEntry>> GetVerwalterrollen()
        {
            var list = Enum.GetValues(typeof(VerwalterRolle))
                .Cast<VerwalterRolle>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/userrole")]
        [Authorize(Policy = "RequireAdmin")]
        public ActionResult<IEnumerable<SelectionEntry>> GetUserRole()
        {
            var list = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/betriebskostenrechnungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetBetriebskostenrechnungen()
        {
            var list = await BetriebskostenrechnungPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.BetriebskostenrechnungId,
                $"{e.BetreffendesJahr} - {e.Umlage.Typ.Bezeichnung} - {e.Umlage.GetWohnungenBezeichnung()}")));
        }

        [HttpGet]
        [Route("api/selection/erhaltungsaufwendungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetErhaltungsaufwendungen()
        {
            var list = await ErhaltungsaufwendungPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.ErhaltungsaufwendungId,
                $"{e.Bezeichnung} - {e.Aussteller.Bezeichnung}")));
        }

        [HttpGet]
        [Route("api/selection/mieten")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetMieten()
        {
            var list = await MietePermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.MieteId,
                $"{e.Vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannt"} - {e.Vertrag.Wohnung.Bezeichnung} - {e.BetreffenderMonat.ToString("MM.yyyy")}")));
        }

        [HttpGet]
        [Route("api/selection/mietminderungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetMietminderungen()
        {
            var list = await MietminderungPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.MietminderungId,
                $"{e.Vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannt"} - {e.Vertrag.Wohnung.Bezeichnung} - {e.Beginn.ToString("MM.yyyy")}")));
        }

        [HttpGet]
        [Route("api/selection/umlagen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetUmlagen()
        {
            var list = await UmlagePermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.UmlageId,
                $"{e.Typ.Bezeichnung} - {e.Wohnungen.ToList().GetWohnungenBezeichnung()}",
                e.Typ.UmlagetypId.ToString())));
        }

        [HttpGet]
        [Route("api/selection/umlagen_wohnungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetUmlagenWohnungen()
        {
            var list = await UmlagePermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.UmlageId,
                e.Wohnungen.ToList().GetWohnungenBezeichnung(),
                e.Typ.UmlagetypId.ToString())));
        }

        [HttpGet]
        [Route("api/selection/umlagen_verbrauch")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetUmlagenVerbrauch()
        {
            var list = await UmlagePermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list
                .Where(e => e.Schluessel == Umlageschluessel.NachVerbrauch)
                .Select(e => new SelectionEntry(
                    e.UmlageId,
                    e.Typ.Bezeichnung + " - " + e.Wohnungen.ToList().GetWohnungenBezeichnung())));
        }

        [HttpGet]
        [Route("api/selection/kontakte")]
        public ActionResult<IEnumerable<SelectionEntry>> GetKontakte()
        {
            var list = Ctx.Kontakte
                .Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung, null))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/juristischepersonen")]
        public ActionResult<IEnumerable<SelectionEntry>> GetJuristischePersonen()
        {
            var list = Ctx.Kontakte
                .Where(e => e.Rechtsform != Rechtsform.natuerlich)
                .Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung, null))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/wohnungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetWohnungen()
        {
            var list = await WohnungPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.WohnungId,
                $"{e.Adresse?.Anschrift ?? ""} - {e.Bezeichnung}",
                e.Besitzer?.Bezeichnung)));
        }

        [HttpGet]
        [Route("api/selection/vertraege")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetVertraege()
        {
            var list = await VertragPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(vertrag => new SelectionEntry(vertrag.VertragId, GetVertragName(vertrag))));
        }

        private static string GetVertragName(Vertrag vertrag)
        {
            var wohnung = $"{vertrag.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift"} - {vertrag.Wohnung.Bezeichnung}";
            var mieterList = vertrag.Mieter.Select(person => person.Bezeichnung);
            var mieterText = string.Join(", ", mieterList);
            var vertragname = $"{wohnung} - {mieterText}";

            return vertragname;
        }

        [HttpGet]
        [Route("api/selection/zaehler")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetZaehler()
        {
            var list = await ZaehlerPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(zaehler => new SelectionEntry(zaehler.ZaehlerId, getZaehlerName(zaehler))));
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
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetZaehlerStaende()
        {
            var list = await ZaehlerstandPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(stand => new SelectionEntry(
                stand.ZaehlerstandId,
                $"{getZaehlerName(stand.Zaehler)} - {stand.Datum.ToString("dd.MM.yyyy")}")));
        }

        [HttpGet]
        [Route("api/selection/umlagetypen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetUmlagetypen()
        {
            var list = await UmlagetypPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(typ => new SelectionEntry(typ.UmlagetypId, typ.Bezeichnung)));
        }

        [HttpGet]
        [Route("api/selection/umlageschluessel")]
        public ActionResult<IEnumerable<SelectionEntry>> GetUmlageschluessel()
        {
            var list = Enum.GetValues(typeof(Umlageschluessel))
                .Cast<Umlageschluessel>()
                .ToList()
                .Select(t => new SelectionEntry((int)t, t.ToDescriptionString()))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/hkvo_p9a2")]
        public ActionResult<IEnumerable<SelectionEntry>> GetHKVO_P9A2()
        {
            var list = Enum.GetValues(typeof(HKVO_P9A2))
                .Cast<HKVO_P9A2>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToDescriptionString()))
                .ToList();

            return Ok(list);
        }

        [HttpGet]
        [Route("api/selection/zaehlertypen")]
        public ActionResult<IEnumerable<SelectionEntry>> GetZaehlertypen()
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
        public ActionResult<IEnumerable<SelectionEntry>> GetAnreden()
        {
            var list = Enum.GetValues(typeof(Anrede))
                .Cast<Anrede>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToString()))
                .ToList();

            return new OkObjectResult(list);
        }

        [HttpGet]
        [Route("api/selection/rechtsformen")]
        public ActionResult<IEnumerable<SelectionEntry>> GetRechtsformen()
        {
            var list = Enum.GetValues(typeof(Rechtsform))
                .Cast<Rechtsform>()
                .ToList()
                .Select(e => new SelectionEntry((int)e, e.ToDescriptionString()))
                .ToList();

            return new OkObjectResult(list);
        }
    }
}
