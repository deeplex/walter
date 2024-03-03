// Copyright (c) 2023-2024 Kai Lawrence
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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MieteController;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragController;
using static Deeplex.Saverwalter.WebAPI.Controllers.WohnungController;
using static Deeplex.Saverwalter.WebAPI.Controllers.ZaehlerController;

namespace Deeplex.Saverwalter.WebAPI.Controllers.Utils
{
    public class BetriebskostenabrechnungController : ControllerBase
    {
        private readonly ILogger<BetriebskostenabrechnungController> _logger;
        private BetriebskostenabrechnungHandler Service { get; }

        public class VerbrauchEntry
        {
            public SelectionEntry Zaehler { get; }
            public double Delta { get; }

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
            public Dictionary<Zaehlereinheit, double> AlleVerbrauch { get; }
            public Dictionary<Zaehlereinheit, List<VerbrauchEntry>> AlleZaehler { get; } = new();
            public Dictionary<Zaehlereinheit, double> DieseVerbrauch { get; }
            public Dictionary<Zaehlereinheit, List<VerbrauchEntry>> DieseZaehler { get; } = new();
            public Dictionary<Zaehlereinheit, double> Anteil { get; }

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
            public int Id { get; }
            public int RechnungId { get; }
            public string Typ { get; }
            public int TypId { get; }
            public string Schluessel { get; }
            public double GesamtBetrag { get; }
            public double Anteil { get; }
            public double Betrag { get; }
            public double BetragLetztesJahr { get; }
            public string Beschreibung { get; }

            public RechnungEntry(KeyValuePair<Umlage, Betriebskostenrechnung?> rechnung, Abrechnungseinheit einheit, int year)
            {
                Id = rechnung.Key.UmlageId;
                RechnungId = rechnung.Value?.BetriebskostenrechnungId ?? 0;
                Typ = rechnung.Key.Typ.Bezeichnung;
                TypId = rechnung.Key.Typ.UmlagetypId;
                var key = rechnung.Key.Schluessel;
                Schluessel = key.ToDescriptionString();
                GesamtBetrag = rechnung.Value?.Betrag ?? 0;
                Anteil = einheit.GetAnteil(rechnung.Key);
                Betrag = GesamtBetrag * Anteil;
                BetragLetztesJahr = rechnung.Key.Betriebskostenrechnungen
                    .Where(bkr => (bkr.BetreffendesJahr + 1) == year)
                    .Sum(bkr => bkr.Betrag);
                Beschreibung = rechnung.Key.Beschreibung ?? "";
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
            public List<Heizkostenberechnung>? Heizkostenberechnungen { get; }
            public double AllgStromFaktor { get; }

            public AbrechnungseinheitEntry(Abrechnungseinheit einheit, int year)
            {
                Bezeichnung = einheit.Bezeichnung;
                GesamtWohnflaeche = einheit.GesamtWohnflaeche;
                GesamtNutzflaeche = einheit.GesamtNutzflaeche;
                GesamtEinheiten = einheit.GesamtEinheiten;
                WFZeitanteil = einheit.WFZeitanteil;
                NFZeitanteil = einheit.NFZeitanteil;
                NEZeitanteil = einheit.NEZeitanteil;
                PersonenZeitanteil = einheit.PersonenZeitanteile;
                Heizkostenberechnungen = einheit.Heizkostenberechnungen;
                Rechnungen = einheit.Rechnungen
                    .Where(rechnung => rechnung.Key.HKVO == null)
                    .Select(rechnung => new RechnungEntry(rechnung, einheit, year))
                    .ToList();
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
            public List<AbrechnungseinheitEntry>? Abrechnungseinheiten { get; }
            public double Result { get; }
            public Zeitraum? Zeitraum { get; set; }
            public List<WohnungEntryBase> Wohnungen { get; }
            public List<VertragEntryBase> Vertraege { get; }
            public List<ZaehlerEntryBase> Zaehler { get; }
            public List<MieteEntryBase> Mieten { get; }

            public BetriebskostenabrechnungEntry(Betriebskostenabrechnung abrechnung)
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
            }
        }

        public BetriebskostenabrechnungController(ILogger<BetriebskostenabrechnungController> logger, BetriebskostenabrechnungHandler service)
        {
            Service = service;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}")]
        public ActionResult<BetriebskostenabrechnungEntry> GetBetriebskostenabrechnung(int vertrag_id, int jahr)
        {
            return Service.Get(vertrag_id, jahr);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/word_document")]
        public ActionResult<MemoryStream> GetBetriebskostenabrechnungWordDocument(int vertrag_id, int jahr)
        {
            return Service.GetWordDocument(vertrag_id, jahr);
        }

        [HttpGet]
        [Route("api/betriebskostenabrechnung/{vertrag_id}/{jahr}/pdf_document")]
        public ActionResult<MemoryStream> GetBetriebskostenabrechnungPdfDocument(int vertrag_id, int jahr)
        {
            return Service.GetPdfDocument(vertrag_id, jahr);
        }
    }
}
