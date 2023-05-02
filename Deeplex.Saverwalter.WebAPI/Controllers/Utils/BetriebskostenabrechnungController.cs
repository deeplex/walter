using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AdresseController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.UmlageController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class BetriebskostenabrechnungController : ControllerBase
    {
        private readonly ILogger<BetriebskostenabrechnungController> _logger;
        private BetriebskostenabrechnungHandler Service { get; }

        public class RechnungsgruppeEntry
        {
            public List<UmlageEntry>? Umlagen { get; }

            public string? Bezeichnung { get; }
            public double GesamtWohnflaeche { get; }
            public double WFZeitanteil { get; }
            public double NFZeitanteil { get; }
            public double GesamtNutzflaeche { get; }
            public int GesamtEinheiten { get; }
            public double NEZeitanteil { get; }
            public List<PersonenZeitIntervall>? GesamtPersonenIntervall { get; }
            public List<PersonenZeitIntervall>? PersonenIntervall { get; }
            public List<PersonenZeitanteil>? PersonenZeitanteil { get; }

            public List<Heizkostenberechnung>? Heizkosten { get; }
            public double GesamtBetragKalt { get; }
            public double BetragKalt { get; }
            public double GesamtBetragWarm { get; }
            public double BetragWarm { get; }

            public RechnungsgruppeEntry(IRechnungsgruppe g, SaverwalterContext ctx)
            {
                Umlagen = g.Umlagen.Select(e => new UmlageEntry(e, ctx)).ToList();
                Bezeichnung = g.Bezeichnung;
                GesamtWohnflaeche = g.GesamtWohnflaeche;
                WFZeitanteil = g.WFZeitanteil;
                NFZeitanteil = g.NFZeitanteil;
                GesamtNutzflaeche = g.GesamtNutzflaeche;
                GesamtEinheiten = g.GesamtEinheiten;
                NEZeitanteil = g.NEZeitanteil;
                GesamtPersonenIntervall = g.GesamtPersonenIntervall;
                PersonenIntervall = g.PersonenIntervall;
                PersonenZeitanteil = g.PersonenZeitanteil;
                GesamtBetragKalt = g.GesamtBetragKalt;
                GesamtBetragWarm = g.GesamtBetragWarm;
                BetragWarm = g.BetragWarm;
                BetragKalt = g.BetragKalt;
            }
        }

        public class BetriebskostenabrechnungEntry
        {
            public List<Note> notes { get; } = new List<Note>();
            public int Jahr { get; set; }
            public DateOnly Abrechnungsbeginn { get; set; }
            public DateOnly Abrechnungsende { get; set; }
            public SelectionEntry? Vermieter { get; }
            public SelectionEntry? Ansprechpartner { get; }
            public List<SelectionEntry>? Mieter { get; }
            public SelectionEntry? Vertrag { get; }
            public SelectionEntry? Wohnung { get; }
            public AdresseEntryBase? Adresse { get; }
            public double Gezahlt { get; }
            public double KaltMiete { get; }
            public double BetragNebenkosten { get; }
            public double BezahltNebenkosten { get; }
            public double Minderung { get; }
            public double NebenkostenMinderung { get; }
            public double KaltMinderung { get; }
            public DateOnly Nutzungsbeginn { get; }
            public DateOnly Nutzungsende { get; }
            public List<SelectionEntry>? Zaehler { get; }
            public int Abrechnungszeitspanne { get; }
            public int Nutzungszeitspanne { get; }
            public double Zeitanteil { get; }
            public List<RechnungsgruppeEntry>? Gruppen { get; }
            public double Result { get; }
            public double AllgStromFaktor { get; set; }

            public BetriebskostenabrechnungEntry(IBetriebskostenabrechnung b, SaverwalterContext ctx)
            {
                notes = b.Notes;
                Jahr = b.Jahr;
                Abrechnungsbeginn = b.Abrechnungsbeginn;
                Abrechnungsende = b.Abrechnungsende;
                //Versionen = b.Versionen;
                Vermieter = new SelectionEntry(b.Vermieter.PersonId, b.Vermieter.Bezeichnung);
                Ansprechpartner = new SelectionEntry(b.Ansprechpartner.PersonId, b.Ansprechpartner.Bezeichnung);
                Mieter = b.Mieter.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
                Vertrag = new SelectionEntry(b.Vertrag.VertragId, "Vertrag");
                Wohnung = new SelectionEntry(b.Wohnung.WohnungId, b.Wohnung.Bezeichnung);
                Adresse = new AdresseEntryBase(b.Adresse);
                Gezahlt = b.Gezahlt;
                KaltMiete = b.KaltMiete;
                Minderung = b.Minderung;
                NebenkostenMinderung = b.NebenkostenMinderung;
                KaltMinderung = b.KaltMinderung;
                Nutzungsbeginn = b.Nutzungsbeginn;
                Nutzungsende = b.Nutzungsende;
                Zaehler = b.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer)).ToList();
                Abrechnungszeitspanne = b.Abrechnungszeitspanne;
                Nutzungszeitspanne = b.Nutzungszeitspanne;
                Zeitanteil = b.Zeitanteil;

                Result = b.Result;
                AllgStromFaktor = b.AllgStromFaktor;

                Gruppen = b.Gruppen.Select(e => new RechnungsgruppeEntry(e, ctx)).ToList();
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
