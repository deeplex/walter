using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.BetriebskostenrechnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class BetriebskostenabrechnungController : ControllerBase
    {
        private readonly ILogger<BetriebskostenabrechnungController> _logger;
        private BetriebskostenabrechnungHandler Service { get; }

        public class VerbrauchAnteilEntry
        {
            public SelectionEntry Umlage { get; }
            public Dictionary<Zaehlereinheit, double> AlleVerbrauch { get; }
            public Dictionary<Zaehlereinheit, double> DieseVerbrauch { get; }
            public Dictionary<Zaehlereinheit, double> Anteil { get; }

            public VerbrauchAnteilEntry(VerbrauchAnteil anteil)
            {
                Umlage = new SelectionEntry(anteil.Umlage.UmlageId, anteil.Umlage.Typ.ToDescriptionString());
                AlleVerbrauch = anteil.AlleVerbrauch;
                DieseVerbrauch = anteil.DieseVerbrauch;
                Anteil = anteil.Anteil;
            }
        }

        public class RechnungEntry
        {
            public int Id { get; }
            public int RechnungId { get; }
            public string Typ { get; }
            public int TypId { get; }
            public string Schluessel { get; }
            public double GesamtBetrag { get; }
            public double Anteil { get; }
            public double Betrag { get; }

            public RechnungEntry(KeyValuePair<Umlage, Betriebskostenrechnung?> rechnung, Abrechnungseinheit einheit)
            {
                Id = rechnung.Key.UmlageId;
                RechnungId = rechnung.Value?.BetriebskostenrechnungId ?? 0;
                Typ = rechnung.Key.Typ.ToDescriptionString();
                TypId = (int)rechnung.Key.Typ;
                var key = rechnung.Key.Schluessel;
                Schluessel = key.ToDescriptionString();
                GesamtBetrag = rechnung.Value?.Betrag ?? 0;
                Anteil = einheit.GetAnteil(rechnung.Key);
                Betrag = GesamtBetrag * Anteil;
            }
        }

        public class AbrechnungseinheitEntry
        {
            public List<RechnungEntry>? Rechnungen { get; }
            public double BetragKalt { get; }
            public double BetragWarm { get; }
            public double GesamtBetragKalt { get; }
            public double GesamtBetragWarm { get; }
            public string? Bezeichnung { get; }
            public double GesamtWohnflaeche { get; }
            public double GesamtNutzflaeche { get; }
            public int GesamtEinheiten { get; }
            public double WFZeitanteil { get; }
            public double NFZeitanteil { get; }
            public double NEZeitanteil { get; }
            public List<VerbrauchAnteilEntry>? VerbrauchAnteil { get; }
            public List<PersonenZeitanteil>? PersonenZeitanteil { get; }
            // TODO not in use yet...
            // public List<Heizkostenberechnung>? Heizkostenberechnungen { get; }
            public double AllgStromFaktor { get; }

            public AbrechnungseinheitEntry(Abrechnungseinheit einheit)
            {
                Bezeichnung = einheit.Bezeichnung;
                GesamtWohnflaeche = einheit.GesamtWohnflaeche;
                GesamtNutzflaeche = einheit.GesamtNutzflaeche;
                GesamtEinheiten = einheit.GesamtEinheiten;
                WFZeitanteil = einheit.WFZeitanteil;
                NFZeitanteil = einheit.NFZeitanteil;
                NEZeitanteil = einheit.NEZeitanteil;
                Rechnungen = einheit.Rechnungen
                    .Where(rechnung => (int)rechnung.Key.Typ % 2 == 0)
                    .Select(rechnung => new RechnungEntry(rechnung, einheit))
                    .ToList();
                PersonenZeitanteil = einheit.PersonenZeitanteile;
                VerbrauchAnteil = einheit.VerbrauchAnteile
                    .Select(anteil => new VerbrauchAnteilEntry(anteil))
                    .ToList();
                BetragKalt = einheit.BetragKalt;
                GesamtBetragKalt = einheit.GesamtBetragKalt;
                BetragWarm = einheit.BetragWarm;
                GesamtBetragWarm = einheit.GesamtBetragWarm;
                AllgStromFaktor = einheit.AllgStromFaktor;
            }
        }

        public class BetriebskostenabrechnungEntry
        {
            public List<Note> Notes { get; } = new List<Note>();
            public SelectionEntry? Vermieter { get; }
            public SelectionEntry? Ansprechpartner { get; }
            public List<SelectionEntry>? Mieter { get; }
            public SelectionEntry? Vertrag { get; }
            public double GezahltMiete { get; }
            public double KaltMiete { get; }
            public double BetragNebenkosten { get; }
            public double BezahltNebenkosten { get; }
            public double NebenkostenMietminderung { get; }
            public double KaltMietminderung { get; }
            public double Mietminderung { get; }
            public List<ZaehlerEntryBase>? Zaehler { get; }
            public List<AbrechnungseinheitEntry>? Abrechnungseinheiten { get; }
            public double Result { get; }
            public Zeitraum? Zeitraum { get; set; }

            public BetriebskostenabrechnungEntry(Betriebskostenabrechnung abrechnung)
            {
                Notes = abrechnung.Notes;
                Vermieter = new SelectionEntry(abrechnung.Vermieter.PersonId, abrechnung.Vermieter.Bezeichnung);
                Ansprechpartner = new SelectionEntry(abrechnung.Ansprechpartner.PersonId, abrechnung.Ansprechpartner.Bezeichnung);
                Mieter = abrechnung.Mieter.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
                Vertrag = new SelectionEntry(abrechnung.Vertrag.VertragId, "Vertrag");
                GezahltMiete = abrechnung.GezahlteMiete;
                KaltMiete = abrechnung.KaltMiete;
                Mietminderung = abrechnung.Mietminderung;
                NebenkostenMietminderung = abrechnung.NebenkostenMietminderung;
                KaltMietminderung = abrechnung.KaltMietminderung;
                Zeitraum = abrechnung.Zeitraum;
                Result = abrechnung.Result;
                Abrechnungseinheiten = abrechnung.Abrechnungseinheiten
                    .Select(einheit => new AbrechnungseinheitEntry(einheit))
                    .ToList();
                Zaehler = abrechnung.Vertrag.Wohnung.Zaehler.Select(e => new ZaehlerEntryBase(e)).ToList();
            }
        }

        public BetriebskostenabrechnungController(ILogger<BetriebskostenabrechnungController> logger, BetriebskostenabrechnungHandler service)
        {
            Service = service;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}")]
        public IActionResult GetBetriebskostenabrechnung(int vertrag_id, int jahr)
        {
            return Service.Get(vertrag_id, jahr, Service.Ctx);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/word_document")]
        public IActionResult GetBetriebskostenabrechnungWordDocument(int vertrag_id, int jahr)
        {
            return Service.GetWordDocument(vertrag_id, jahr, Service.Ctx);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/pdf_document")]
        public IActionResult GetBetriebskostenabrechnungPdfDocument(int vertrag_id, int jahr)
        {
            return Service.GetPdfDocument(vertrag_id, jahr, Service.Ctx);
        }
    }
}
