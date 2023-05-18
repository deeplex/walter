﻿using Deeplex.Saverwalter.BetriebskostenabrechnungService;
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

            public AbrechnungseinheitEntry(IAbrechnungseinheit g, SaverwalterContext ctx)
            {
                Bezeichnung = g.Bezeichnung;
                PersonenIntervall = g.PersonenIntervall;
                GesamtPersonenIntervall = g.GesamtPersonenIntervall;
                GesamtWohnflaeche = g.GesamtWohnflaeche;
                GesamtNutzflaeche = g.GesamtNutzflaeche;
                GesamtEinheiten = g.GesamtEinheiten;
                WFZeitanteil = g.WFZeitanteil;
                NFZeitanteil = g.NFZeitanteil;
                NEZeitanteil = g.NEZeitanteil;
                Umlagen = g.Umlagen.Select(e => new UmlageEntry(e, ctx)).ToList();
                PersonenZeitanteil = g.PersonenZeitanteil;
                Verbrauch = g.Verbrauch;
                VerbrauchAnteil = g.VerbrauchAnteil;
                BetragKalteNebenkosten = g.BetragKalteNebenkosten;
                GesamtBetragKalteNebenkosten = g.GesamtBetragKalteNebenkosten;
                BetragWarmeNebenkosten = g.BetragWarmeNebenkosten;
                GesamtBetragWarmeNebenkosten = g.GesamtBetragWarmeNebenkosten;
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

            public BetriebskostenabrechnungEntry(IBetriebskostenabrechnung b, SaverwalterContext ctx)
            {
                notes = b.Notes;
                //Versionen = b.Versionen;
                Vermieter = new SelectionEntry(b.Vermieter.PersonId, b.Vermieter.Bezeichnung);
                Ansprechpartner = new SelectionEntry(b.Ansprechpartner.PersonId, b.Ansprechpartner.Bezeichnung);
                Mieter = b.Mieter.Select(e => new SelectionEntry(e.PersonId, e.Bezeichnung)).ToList();
                Vertrag = new SelectionEntry(b.Vertrag.VertragId, "Vertrag");
                Wohnung = new SelectionEntry(b.Wohnung.WohnungId, b.Wohnung.Bezeichnung);
                Adresse = new AdresseEntryBase(b.Adresse);
                Gezahlt = b.GezahlteMiete;
                KaltMiete = b.KaltMiete;
                Minderung = b.Mietminderung;
                NebenkostenMinderung = b.NebenkostenMietminderung;
                KaltMinderung = b.KaltMietminderung;
                Zaehler = b.Zaehler.Select(e => new SelectionEntry(e.ZaehlerId, e.Kennnummer)).ToList();
                //Zeitraum

                Result = b.Result;
                AllgStromFaktor = b.AllgStromFaktor;

                Abrechnungseinheiten = b.Abrechnungseinheiten.Select(e => new AbrechnungseinheitEntry(e, ctx)).ToList();
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
