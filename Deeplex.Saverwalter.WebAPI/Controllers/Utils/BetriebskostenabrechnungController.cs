// Copyright (c) 2023-2025 Kai Lawrence
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

using System.Security;
using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class BetriebskostenabrechnungController : ControllerBase
    {
        private readonly ILogger<BetriebskostenabrechnungController> _logger;
        private BetriebskostenabrechnungHandler Service { get; }

        public class VerbrauchEntry
        {
            public SelectionEntry Zaehler { get; }
            public decimal Delta { get; }

            public VerbrauchEntry(Verbrauch verbrauch)
            {
                Zaehler = new SelectionEntry(
                    verbrauch.Zaehler.ZaehlerId,
                    verbrauch.Zaehler.Kennnummer,
                    verbrauch.Zaehler.Typ.ToUnitString());
                Delta = verbrauch.Delta;
            }
        }

        public class VerbrauchAnteilEntry
        {
            public SelectionEntry Umlage { get; }
            public Dictionary<Zaehlereinheit, decimal> AlleVerbrauch { get; }
            public Dictionary<Zaehlereinheit, List<VerbrauchEntry>> AlleZaehler { get; } = new();
            public Dictionary<Zaehlereinheit, decimal> DieseVerbrauch { get; }
            public Dictionary<Zaehlereinheit, List<VerbrauchEntry>> DieseZaehler { get; } = new();
            public Dictionary<Zaehlereinheit, decimal> Anteil { get; }

            public VerbrauchAnteilEntry(VerbrauchAnteil anteil)
            {
                Umlage = new SelectionEntry(anteil.Umlage.UmlageId, anteil.Umlage.Typ.Bezeichnung);
                AlleVerbrauch = anteil.AlleVerbrauch;
                DieseVerbrauch = anteil.DieseVerbrauch;
                Anteil = anteil.Anteil;
                foreach (var zaehler in anteil.AlleZaehler)
                {
                    if (!AlleZaehler.ContainsKey(zaehler.Key))
                    {
                        AlleZaehler[zaehler.Key] = new();
                    }

                    AlleZaehler[zaehler.Key].AddRange(zaehler.Value
                        .Select(verbrauch => new VerbrauchEntry(verbrauch)));
                }

                foreach (var zaehler in anteil.DieseZaehler)
                {
                    if (!DieseZaehler.ContainsKey(zaehler.Key))
                    {
                        DieseZaehler[zaehler.Key] = new();
                    }

                    DieseZaehler[zaehler.Key].AddRange(zaehler.Value
                        .Select(verbrauch => new VerbrauchEntry(verbrauch)));
                }
            }
        }

        public class RechnungEntry
        {
            public int UmlageId { get; }
            public int RechnungId { get; }
            public string Typ { get; }
            public int TypId { get; }
            public string Schluessel { get; }
            public decimal GesamtBetrag { get; }
            public decimal Anteil { get; }
            public decimal Betrag { get; }
            public decimal BetragLetztesJahr { get; }
            public string Beschreibung { get; }

            public RechnungEntry(Umlage umlage, BetriebskostenrechnungEntry rechnung, Abrechnungseinheit einheit, int year)
            {
                UmlageId = umlage.UmlageId;
                RechnungId = rechnung.Rechnung?.BetriebskostenrechnungId ?? 0;
                Typ = umlage.Typ.Bezeichnung;
                TypId = umlage.Typ.UmlagetypId;
                var key = umlage.Schluessel;
                Schluessel = key.ToDescriptionString();
                GesamtBetrag = rechnung?.Betrag ?? 0;
                Anteil = einheit.GetAnteil(umlage);
                Betrag = GesamtBetrag * Anteil;
                BetragLetztesJahr = umlage.Betriebskostenrechnungen
                    .Where(bkr => (bkr.BetreffendesJahr + 1) == year)
                    .Sum(bkr => bkr.Betrag);
                Beschreibung = umlage.Beschreibung ?? "";
            }
        }

        public class AbrechnungseinheitEntry
        {
            public List<RechnungEntry>? Rechnungen { get; }
            public decimal BetragKalt { get; }
            public decimal BetragWarm { get; }
            public decimal GesamtBetragKalt { get; }
            public decimal GesamtBetragWarm { get; }
            public string? Bezeichnung { get; }
            public decimal GesamtWohnflaeche { get; }
            public decimal GesamtNutzflaeche { get; }
            public decimal GesamtMiteigentumsanteile { get; }
            public int GesamtEinheiten { get; }
            public decimal WFZeitanteil { get; }
            public decimal NFZeitanteil { get; }
            public decimal MEAZeitanteil { get; }
            public decimal NEZeitanteil { get; }
            public List<VerbrauchAnteilEntry>? VerbrauchAnteil { get; }
            public List<PersonenZeitanteil>? PersonenZeitanteil { get; }
            public List<Heizkostenberechnung>? Heizkostenberechnungen { get; }

            public AbrechnungseinheitEntry(Abrechnungseinheit einheit, int year)
            {
                Bezeichnung = einheit.Bezeichnung;
                GesamtWohnflaeche = einheit.GesamtWohnflaeche;
                GesamtNutzflaeche = einheit.GesamtNutzflaeche;
                GesamtMiteigentumsanteile = einheit.GesamtMiteigentumsanteile;
                GesamtEinheiten = einheit.GesamtEinheiten;
                WFZeitanteil = einheit.WFZeitanteil;
                NFZeitanteil = einheit.NFZeitanteil;
                MEAZeitanteil = einheit.MEAZeitanteil;
                NEZeitanteil = einheit.NEZeitanteil;
                PersonenZeitanteil = einheit.PersonenZeitanteile;
                Heizkostenberechnungen = einheit.Heizkostenberechnungen;
                Rechnungen = [.. einheit.Rechnungen
                    .Where(rechnung => rechnung.Key.HKVO == null)
                    .SelectMany(rechnungen =>
                        rechnungen.Value.Select(
                            rechnung => new RechnungEntry(rechnungen.Key, rechnung, einheit, year)))];
                VerbrauchAnteil = [.. einheit.VerbrauchAnteile.Select(anteil => new VerbrauchAnteilEntry(anteil))];
                BetragKalt = einheit.BetragKalt;
                GesamtBetragKalt = einheit.GesamtBetragKalt;
                BetragWarm = einheit.BetragWarm;
                GesamtBetragWarm = einheit.GesamtBetragWarm;
            }
        }

        public class BetriebskostenabrechnungEntry
        {
            public List<Note> Notes { get; } = new List<Note>();
            public SelectionEntry? Vermieter { get; }
            public SelectionEntry? Ansprechpartner { get; }
            public List<SelectionEntry>? Mieter { get; }
            public SelectionEntry? Vertrag { get; }
            public decimal GezahltMiete { get; }
            public decimal KaltMiete { get; }
            public decimal BetragNebenkosten { get; }
            public decimal BezahltNebenkosten { get; }
            public decimal NebenkostenMietminderung { get; }
            public decimal KaltMietminderung { get; }
            public decimal Mietminderung { get; }
            public List<AbrechnungseinheitEntry>? Abrechnungseinheiten { get; }
            public decimal Result { get; }
            public Zeitraum? Zeitraum { get; set; }
            public List<WohnungEntryBase> Wohnungen { get; }
            public List<VertragEntryBase> Vertraege { get; }
            public List<ZaehlerEntryBase> Zaehler { get; }
            public List<MieteEntryBase> Mieten { get; }
            public AbrechnungsresultatEntryBase? Resultat { get; }

            public BetriebskostenabrechnungEntry(Betriebskostenabrechnung abrechnung, Abrechnungsresultat? resultat)
            {
                Notes = abrechnung.Notes;
                Vermieter = new SelectionEntry(abrechnung.Vermieter.KontaktId, abrechnung.Vermieter.Bezeichnung);
                Ansprechpartner = new SelectionEntry(abrechnung.Ansprechpartner.KontaktId, abrechnung.Ansprechpartner.Bezeichnung);
                Mieter = abrechnung.Mieter.Select(e => new SelectionEntry(e.KontaktId, e.Bezeichnung)).ToList();
                Vertrag = new SelectionEntry(abrechnung.Vertrag.VertragId, "Vertrag");
                GezahltMiete = abrechnung.GezahlteMiete;
                KaltMiete = abrechnung.KaltMiete;
                Mietminderung = abrechnung.Mietminderung;
                NebenkostenMietminderung = abrechnung.NebenkostenMietminderung;
                KaltMietminderung = abrechnung.KaltMietminderung;
                Zeitraum = abrechnung.Zeitraum;
                Result = abrechnung.Result;
                Abrechnungseinheiten = abrechnung.Abrechnungseinheiten
                    .Select(einheit => new AbrechnungseinheitEntry(einheit, Zeitraum.Jahr))
                    .ToList();


                var wohnungen = abrechnung.Abrechnungseinheiten
                    .SelectMany(einheit => einheit.Rechnungen.Select(rechnung => rechnung.Key))
                    .SelectMany(umlage => umlage.Wohnungen)
                    .Distinct();
                Wohnungen = wohnungen
                    .Select(wohnung => new WohnungEntryBase(wohnung, new()))
                    .ToList();
                Vertraege = wohnungen
                    .SelectMany(wohnung => wohnung.Vertraege)
                    .Where(vertrag => vertrag.Beginn() <= abrechnung.Zeitraum.Abrechnungsende &&
                        (vertrag.Ende == null || vertrag.Ende >= abrechnung.Zeitraum.Abrechnungsbeginn))
                    .Select(vertrag => new VertragEntryBase(vertrag, new()))
                    .ToList();
                Zaehler = wohnungen
                    .SelectMany(wohnung => wohnung.Zaehler)
                    .Select(e => new ZaehlerEntryBase(e, new()))
                    .ToList();
                Mieten = abrechnung.Vertrag.Mieten
                    .Where(miete =>
                        miete.BetreffenderMonat >= abrechnung.Zeitraum.Abrechnungsbeginn &&
                        miete.BetreffenderMonat <= abrechnung.Zeitraum.Abrechnungsende)
                    .Select(miete => new MieteEntryBase(miete, new()))
                    .ToList();

                if (resultat != null)
                {
                    var permissions = new Permissions
                    {
                        Read = true,
                        Update = true,
                        Remove = true
                    };
                    Resultat = new AbrechnungsresultatEntry(resultat, permissions);
                }

            }
        }

        public BetriebskostenabrechnungController(ILogger<BetriebskostenabrechnungController> logger, BetriebskostenabrechnungHandler service)
        {
            Service = service;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}")]
        public Task<ActionResult<BetriebskostenabrechnungEntry>> GetBetriebskostenabrechnung(
            int vertrag_id, int jahr)
        {
            return Service.Get(vertrag_id, jahr);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/word_document")]
        public ActionResult<MemoryStream> GetBetriebskostenabrechnungWordDocument(
            int vertrag_id, int jahr)
        {
            return Service.GetWordDocument(vertrag_id, jahr);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/pdf_document")]
        public ActionResult<MemoryStream> GetBetriebskostenabrechnungPdfDocument(int vertrag_id, int jahr)
        {
            return Service.GetPdfDocument(vertrag_id, jahr);
        }

        [HttpPost]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/word_document")]
        public Task<ActionResult<MemoryStream>> GetBetriebskostenabrechnungWordDocumentAndSaveResult(
            int vertrag_id, int jahr)
        {
            return Service.GetWordDocumentAndSaveResult(vertrag_id, jahr);
        }

        [HttpPost]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/pdf_document")]
        public Task<ActionResult<MemoryStream>> GetBetriebskostenabrechnungPdfDocumentAndSaveResult(
            int vertrag_id, int jahr)
        {
            return Service.GetPdfDocumentAndSaveResult(vertrag_id, jahr);
        }
    }
}
