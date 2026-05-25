// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly AbrechnungsgruppenService _gruppenService;

        public SelectionListController(ILogger<SelectionListController> logger, SaverwalterContext ctx, AbrechnungsgruppenService gruppenService)
        {
            Ctx = ctx;
            _logger = logger;
            _gruppenService = gruppenService;
        }

        [HttpGet]
        [Route("api/selection/abrechnungsgruppen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetAbrechnungsgruppen()
        {
            var gruppen = await _gruppenService.GetGruppenAsync();
            return Ok(gruppen.Select(g => new SelectionEntry(g.WohnungIds[0], g.Bezeichnung)));
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
        [Route("api/selection/bk-forderungen/offen")]
        public async Task<ActionResult<IEnumerable<object>>> GetOffeneBkForderungen()
        {
            var umlagen = await Ctx.Umlagen
                .AsSplitQuery()
                .Include(u => u.Typ)
                .Include(u => u.Wohnungen).ThenInclude(w => w.Adresse)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben))
                    .ThenInclude(z => z.Buchungssatz)
                .Include(u => u.NkVerrechnungsKonto)
                    .ThenInclude(k => k.Buchungszeilen.Where(z => z.SollHaben == SollHaben.Haben))
                    .ThenInclude(z => z.AlsHabenZeile)
                    .ThenInclude(a => a.SollZeile)
                .ToListAsync();

            return Ok(umlagen
                .SelectMany(u => u.NkVerrechnungsKonto?.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben)
                    .Where(z =>
                    {
                        var schonGezahlt = z.AlsHabenZeile.Sum(a => a.SollZeile.Betrag);
                        return z.Betrag - schonGezahlt > 0.005m;
                    })
                    .Select(z => new
                    {
                        Id = z.Buchungssatz.BuchungssatzId,
                        Text = $"{z.Buchungssatz.Buchungsjahr} - {u.Typ.Bezeichnung} - {u.GetWohnungenBezeichnung()}"
                    }) ?? [])
                .OrderBy(e => e.Text));
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
                .Where(e => e.Versionen.OrderByDescending(v => v.Beginn).FirstOrDefault()?.Schluessel == Umlageschluessel.NachVerbrauch)
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
        [Route("api/selection/garagen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetGaragen()
        {
            var list = GaragePermissionHandler.GetQueryable(Ctx, User).ToList();
            return Ok(list.Select(e => new SelectionEntry(
                e.GarageId,
                e.Kennung,
                e.Besitzer?.Bezeichnung)));
        }

        [HttpGet]
        [Route("api/selection/garage-vertraege")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetGarageVertraege()
        {
            var list = await GarageVertragPermissionHandler.GetQueryable(Ctx, User)
                .Include(gv => gv.Garage)
                .Include(gv => gv.Mieter)
                .ToListAsync();
            return Ok(list.Select(e =>
            {
                var mieter = e.Mieter.Any()
                    ? string.Join(", ", e.Mieter.Select(m => m.Bezeichnung))
                    : "Leerstand";
                return new SelectionEntry(e.GarageVertragId, $"{e.Garage.Kennung} | {mieter}", null);
            }));
        }

        [HttpGet]
        [Route("api/selection/wohnungen")]
        public async Task<ActionResult<IEnumerable<SelectionEntry>>> GetWohnungen()
        {
            var list = await WohnungPermissionHandler.GetList(Ctx, User, VerwalterRolle.Vollmacht);

            return Ok(list.Select(e => new SelectionEntry(
                e.WohnungId,
                $"{e.Adresse?.Anschrift ?? ""} - {e.Bezeichnung}",
                e.Eigentuemer.FirstOrDefault(ei => ei.Bis == null)?.Kontakt.Bezeichnung)));
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

            return list;
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

            return list;
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

            return list;
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

            return list;
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

            return list;
        }

        [HttpGet]
        [Route("api/selection/buchungskonten")]
        public ActionResult<IEnumerable<SelectionEntry>> GetBuchungskonten()
        {
            var konten = Ctx.Buchungskonten
                .OrderBy(k => k.Kontonummer)
                .Select(k => new { k.BuchungskontoId, k.Kontonummer, k.Bezeichnung })
                .ToList();
            return Ok(konten.Select(k => new SelectionEntry(k.BuchungskontoId, $"{k.Kontonummer} – {k.Bezeichnung}")));
        }

        [HttpGet]
        [Route("api/selection/bankkontos")]
        public ActionResult<IEnumerable<SelectionEntry>> GetBankkontos()
        {
            var list = Ctx.Bankkontos
                .Include(b => b.Besitzer)
                .ToList();
            return Ok(list.Select(b => new SelectionEntry(
                b.BankkontoId,
                BankkontoLabel(b))));
        }

        [HttpGet]
        [Route("api/selection/zahler-bankkonto/buchungssatz/{buchungssatzId}")]
        public async Task<ActionResult<SelectionEntry>> GetZahlerBankkontoForBuchungssatz(Guid buchungssatzId)
        {
            var umlageId = await Ctx.Umlagen
                .Where(u => u.NkVerrechnungsKonto.Buchungszeilen.Any(z =>
                    z.Buchungssatz.BuchungssatzId == buchungssatzId && z.SollHaben == SollHaben.Haben))
                .Select(u => (int?)u.UmlageId)
                .FirstOrDefaultAsync();

            if (umlageId is null) return NotFound();

            return await GetZahlerBankkontoForUmlage(umlageId.Value);
        }

        [HttpGet]
        [Route("api/selection/zahler-bankkonto/umlage/{umlageId}")]
        public async Task<ActionResult<SelectionEntry>> GetZahlerBankkontoForUmlage(int umlageId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var besitzerIds = await Ctx.Umlagen
                .Where(u => u.UmlageId == umlageId)
                .SelectMany(u => u.Wohnungen)
                .SelectMany(w => w.Eigentuemer)
                .Where(e => e.Bis == null || e.Bis >= today)
                .Select(e => e.Kontakt.KontaktId)
                .Distinct()
                .ToListAsync();

            if (besitzerIds.Count == 0) return NotFound();

            var bankkonto = await Ctx.Bankkontos
                .Include(b => b.Besitzer)
                .FirstOrDefaultAsync(b => b.Besitzer.Any(k => besitzerIds.Contains(k.KontaktId)));

            if (bankkonto is null) return NotFound();

            return Ok(new SelectionEntry(bankkonto.BankkontoId, BankkontoLabel(bankkonto)));
        }

        internal static string BankkontoLabel(Bankkonto b)
        {
            var parts = new List<string>();
            var besitzer = b.Besitzer.FirstOrDefault()?.Bezeichnung;
            if (!string.IsNullOrWhiteSpace(besitzer)) parts.Add(besitzer);
            if (!string.IsNullOrWhiteSpace(b.Bank)) parts.Add(b.Bank);
            if (!string.IsNullOrWhiteSpace(b.Iban)) parts.Add(b.Iban);
            return parts.Count > 0 ? string.Join(" – ", parts) : $"Bankkonto {b.BankkontoId}";
        }
    }
}
