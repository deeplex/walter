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

        public class AbrechnungseinheitEntry
        {
            public string? Bezeichnung { get; }
            public List<PersonenZeitIntervall>? PersonenIntervall { get; }
            public List<PersonenZeitIntervall>? GesamtPersonenIntervall { get; }
            public double GesamtWohnflaeche { get; }
            public double GesamtNutzflaeche { get; }
            public int GesamtEinheiten { get; }
            public double WFZeitanteil { get; }
            public double NFZeitanteil { get; }
            public double NEZeitanteil { get; }
            public List<UmlageEntry>? Umlagen { get; }
            public List<PersonenZeitanteil>? PersonenZeitanteil { get; }
            public Dictionary<Betriebskostentyp, List<VerbrauchAnteil>> Verbrauch { get; }
            public Dictionary<Betriebskostentyp, double> VerbrauchAnteil { get; }
            public double BetragKalteNebenkosten { get; }
            public double GesamtBetragKalteNebenkosten { get; }
            public List<Heizkostenberechnung>? Heizkosten { get; }
            public double BetragWarmeNebenkosten { get; }
            public double GesamtBetragWarmeNebenkosten { get; }

            public AbrechnungseinheitEntry(Betriebskostenabrechnung abrechnung, Abrechnungseinheit einheit, SaverwalterContext ctx)
            {
                Bezeichnung = einheit.Bezeichnung;
                PersonenIntervall = abrechnung.PersonenIntervall();
                GesamtPersonenIntervall = abrechnung.GesamtPersonenIntervall(einheit);
                GesamtWohnflaeche = einheit.GesamtWohnflaeche;
                GesamtNutzflaeche = einheit.GesamtNutzflaeche;
                GesamtEinheiten = einheit.GesamtEinheiten;
                WFZeitanteil = abrechnung.WFZeitanteil(einheit);
                NFZeitanteil = abrechnung.NFZeitanteil(einheit);
                NEZeitanteil = abrechnung.NEZeitanteil(einheit);
                Umlagen = einheit.Umlagen.Select(e => new UmlageEntry(e, ctx)).ToList();
                PersonenZeitanteil = abrechnung.PersonenZeitanteil(einheit);
                Verbrauch = abrechnung.Verbrauch(einheit);
                VerbrauchAnteil = abrechnung.VerbrauchAnteil(einheit);
                BetragKalteNebenkosten = abrechnung.BetragKalteNebenkosten(einheit);
                GesamtBetragKalteNebenkosten = abrechnung.GesamtBetragKalteNebenkosten(einheit);
                BetragWarmeNebenkosten = abrechnung.BetragWarmeNebenkosten(einheit);
                GesamtBetragWarmeNebenkosten = abrechnung.GesamtBetragWarmeNebenkosten(einheit);
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
            public List<AbrechnungseinheitEntry>? Abrechnungseinheiten { get; }
            public double Result { get; }
            public double AllgStromFaktor { get; set; }

            public BetriebskostenabrechnungEntry(Betriebskostenabrechnung abrechnung, SaverwalterContext ctx)
            {
                notes = abrechnung.Notes;
                Jahr = abrechnung.Zeitraum.Jahr;
                Abrechnungsbeginn = abrechnung.Zeitraum.Abrechnungsbeginn;
                Abrechnungsende = abrechnung.Zeitraum.Abrechnungsende;
                //Versionen = b.Versionen;
                Vermieter = new SelectionEntry(abrechnung.Vermieter.PersonId, abrechnung.Vermieter.Bezeichnung);
                Ansprechpartner = new SelectionEntry(abrechnung.Ansprechpartner.PersonId, abrechnung.Ansprechpartner.Bezeichnung);
                Mieter = abrechnung.Mieter.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
                Vertrag = new SelectionEntry(abrechnung.Vertrag.VertragId, "Vertrag");
                Wohnung = new SelectionEntry(abrechnung.Wohnung.WohnungId, abrechnung.Wohnung.Bezeichnung);
                Adresse = new AdresseEntryBase(abrechnung.Adresse);
                Gezahlt = abrechnung.GezahlteMiete;
                KaltMiete = abrechnung.KaltMiete;
                Minderung = abrechnung.Mietminderung;
                NebenkostenMinderung = abrechnung.NebenkostenMietminderung;
                KaltMinderung = abrechnung.KaltMietminderung;
                Nutzungsbeginn = abrechnung.Zeitraum.Nutzungsbeginn;
                Nutzungsende = abrechnung.Zeitraum.Nutzungsende;
                Zaehler = abrechnung.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer)).ToList();
                Abrechnungszeitspanne = abrechnung.Zeitraum.Abrechnungszeitraum;
                Nutzungszeitspanne = abrechnung.Zeitraum.Nutzungszeitraum;
                Zeitanteil = abrechnung.Zeitraum.Zeitanteil;


                Result = abrechnung.Result;
                AllgStromFaktor = abrechnung.AllgStromFaktor;

                Abrechnungseinheiten = abrechnung.Abrechnungseinheiten.Select(e => new AbrechnungseinheitEntry(abrechnung, e, ctx)).ToList();
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
