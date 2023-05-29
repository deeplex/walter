using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;
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
                GesamtWohnflaeche = einheit.GesamtWohnflaeche;
                GesamtNutzflaeche = einheit.GesamtNutzflaeche;
                GesamtEinheiten = einheit.GesamtEinheiten;
                WFZeitanteil = WFZeitanteil(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum);
                NFZeitanteil = NFZeitanteil(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum);
                NEZeitanteil = NEZeitanteil(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum);
                Umlagen = einheit.Umlagen.Select(e => new UmlageEntry(e, ctx)).ToList();
                PersonenZeitanteil = GetPersonenZeitanteil(abrechnung.Vertrag, einheit, abrechnung.Zeitraum);
                Verbrauch = Verbrauch(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum, abrechnung.Notes);
                VerbrauchAnteil = VerbrauchAnteil(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum, abrechnung.Notes);
                BetragKalteNebenkosten = BetragKalteNebenkosten(abrechnung.Vertrag, einheit, abrechnung.Zeitraum, abrechnung.Notes);
                GesamtBetragKalteNebenkosten = GetKalteNebenkosten(einheit, abrechnung.Zeitraum).Sum(r => r.Betrag);
                BetragWarmeNebenkosten = BetragWarmeNebenkosten(abrechnung.Vertrag, einheit, abrechnung.Zeitraum, abrechnung.Notes);
                GesamtBetragWarmeNebenkosten = GesamtBetragWarmeNebenkosten(abrechnung.Vertrag.Wohnung, einheit, abrechnung.Zeitraum, abrechnung.Notes);
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
                Wohnung = new SelectionEntry(abrechnung.Vertrag.Wohnung.WohnungId, abrechnung.Vertrag.Wohnung.Bezeichnung);
                Adresse = new AdresseEntryBase(abrechnung.Vertrag.Wohnung.Adresse!);
                Gezahlt = abrechnung.GezahlteMiete;
                KaltMiete = abrechnung.KaltMiete;
                Minderung = abrechnung.Mietminderung;
                NebenkostenMinderung = abrechnung.NebenkostenMietminderung;
                KaltMinderung = abrechnung.KaltMietminderung;
                Nutzungsbeginn = abrechnung.Zeitraum.Nutzungsbeginn;
                Nutzungsende = abrechnung.Zeitraum.Nutzungsende;
                Zaehler = abrechnung.Vertrag.Wohnung.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer)).ToList();
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
