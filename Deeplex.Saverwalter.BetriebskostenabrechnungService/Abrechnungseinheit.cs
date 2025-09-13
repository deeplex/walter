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

using System.Reflection.Metadata;
using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public class BetriebskostenrechnungEntry
    {
        public Betriebskostenrechnung? Rechnung { get; }
        public double Betrag { get; }
        public BetriebskostenrechnungEntry(
            Betriebskostenrechnung? rechnung,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            Rechnung = rechnung;

            if (rechnung != null)
            {
                var abzug = 0.0;
                foreach (var hkvo in rechnung.Umlage.HKVOs)
                {
                    var rechnungen = hkvo.Heizkosten.Betriebskostenrechnungen.Where(r
                        => r.BetreffendesJahr == zeitraum.Jahr).ToList();
                    foreach (var r in rechnungen)
                    {
                        abzug += r.Betrag * hkvo.Strompauschale;
                    }
                }

                Betrag = rechnung.Betrag - abzug;

                if (Betrag < 0)
                {
                    notes.Add($"Pauschale der Heizkosten ist mehr als Allgemeinstromrechnung {Betrag:N2}€", Severity.Warning);
                }


            }
        }
    }

    public class Abrechnungseinheit
    {
        public Dictionary<Umlage, List<BetriebskostenrechnungEntry>> Rechnungen { get; } = [];
        public double BetragKalt { get; }
        public double BetragWarm { get; }
        public double GesamtBetragKalt { get; }
        public double GesamtBetragWarm { get; }
        public string Bezeichnung { get; }
        public double GesamtWohnflaeche { get; }
        public double GesamtNutzflaeche { get; }
        public double GesamtMiteigentumsanteile { get; }
        public int GesamtEinheiten { get; }
        public double WFZeitanteil { get; }
        public double NFZeitanteil { get; }
        public double MEAZeitanteil { get; }
        public double NEZeitanteil { get; }
        public List<VerbrauchAnteil> VerbrauchAnteile { get; }
        public List<PersonenZeitanteil> PersonenZeitanteile { get; }
        public List<Heizkostenberechnung> Heizkostenberechnungen { get; }

        public Abrechnungseinheit(List<Umlage> umlagen, Vertrag vertrag, Zeitraum zeitraum, List<Note> notes)
        {
            var wohnungen = umlagen.First().Wohnungen.ToList();
            Bezeichnung = umlagen.First().GetWohnungenBezeichnung();

            GesamtWohnflaeche = wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = wohnungen.Sum(w => w.Nutzeinheit);
            GesamtMiteigentumsanteile = wohnungen.Sum(w => w.Miteigentumsanteile);

            var wohnung = vertrag.Wohnung;
            WFZeitanteil = GesamtWohnflaeche > 0 ?
                wohnung.Wohnflaeche / GesamtWohnflaeche * zeitraum.Zeitanteil : 0;
            NFZeitanteil = GesamtNutzflaeche > 0 ?
                wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil : 0;
            NEZeitanteil = GesamtEinheiten > 0 ?
                (double)wohnung.Nutzeinheit / GesamtEinheiten * zeitraum.Zeitanteil : 0;
            MEAZeitanteil = GesamtMiteigentumsanteile > 0 ?
                wohnung.Miteigentumsanteile / GesamtMiteigentumsanteile * zeitraum.Zeitanteil : 0;

            foreach (var umlage in umlagen)
            {
                Rechnungen[umlage] = [.. umlage.Betriebskostenrechnungen
                    .Where(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr)
                    .Select(rechnung => new BetriebskostenrechnungEntry(rechnung, zeitraum, notes))];

                if (Rechnungen[umlage].Count == 0)
                {
                    notes.Add($"Keine Rechnung für {umlage.Typ.Bezeichnung} gefunden.", Severity.Warning);
                }
            }

            var rechnungenKalt = Rechnungen
                .Where(e => e.Key.HKVO == null && e.Value != null)
                .SelectMany(e => e.Value!)
                .ToList();
            var rechnungenWarm = Rechnungen
                .Where(e => e.Key.HKVO != null && e.Value != null)
                .SelectMany(e => e.Value!)
                .ToList();

            PersonenZeitanteile = PersonenZeitanteil.GetPersonenZeitanteile(vertrag, wohnungen, zeitraum);
            VerbrauchAnteile = VerbrauchAnteil.GetVerbrauchAnteile(umlagen, vertrag, zeitraum, notes);

            Heizkostenberechnungen = CalculateHeizkosten(rechnungenWarm, wohnung, VerbrauchAnteile, zeitraum, notes);
            BetragWarm = Heizkostenberechnungen.Sum(heizkosten => heizkosten.Betrag);

            GesamtBetragWarm = rechnungenWarm.Sum(e => e.Betrag);
            BetragKalt = GetSum(rechnungenKalt, notes);
            GesamtBetragKalt = rechnungenKalt.Sum(rechnung => rechnung.Betrag);
        }

        public double GetAnteil(Umlage umlage)
        {
            return umlage.Schluessel switch
            {
                Umlageschluessel.NachNutzeinheit => NEZeitanteil,
                Umlageschluessel.NachWohnflaeche => WFZeitanteil,
                Umlageschluessel.NachNutzflaeche => NFZeitanteil,
                Umlageschluessel.NachMiteigentumsanteil => MEAZeitanteil,
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteile.Sum(anteil => anteil.Anteil),
                Umlageschluessel.NachVerbrauch => VerbrauchAnteile
                    .SingleOrDefault(anteil => anteil.Umlage == umlage)?.Anteil?.Sum(a => a.Value) ?? 0,
                _ => 0,
            };
        }

        private double GetSum(List<BetriebskostenrechnungEntry> rechnungen, List<Note> notes)
        {
            var PersZeitanteil = PersonenZeitanteile.Sum(z => z.Anteil);

            return rechnungen.Sum(rechnung =>
            {
                if (rechnung.Rechnung == null)
                {
                    return 0;
                }

                return rechnung.Rechnung.Umlage.Schluessel switch
                {
                    Umlageschluessel.NachNutzeinheit => rechnung.Betrag * NEZeitanteil,
                    Umlageschluessel.NachWohnflaeche => rechnung.Betrag * WFZeitanteil,
                    Umlageschluessel.NachNutzflaeche => rechnung.Betrag * NFZeitanteil,
                    Umlageschluessel.NachMiteigentumsanteil => rechnung.Betrag * MEAZeitanteil,
                    Umlageschluessel.NachPersonenzahl => rechnung.Betrag * PersZeitanteil,
                    Umlageschluessel.NachVerbrauch => rechnung.Betrag * GetVerbrauchAnteil(rechnung, notes),
                    _ => 0
                };
            });
        }

        private double GetVerbrauchAnteil(BetriebskostenrechnungEntry rechnung, List<Note> notes)
        {
            var lRechnung = rechnung.Rechnung;
            if (lRechnung == null)
            {
                notes.Add($"Keine Rechnung für {rechnung.Rechnung?.Umlage.Typ.Bezeichnung} gefunden.",
                    Severity.Error);
                return 0;
            }

            var verbrauchAnteile = VerbrauchAnteile.Where(anteil => anteil.Umlage == lRechnung.Umlage).ToList();
            if (verbrauchAnteile.Count == 0)
            {
                notes.Add($"Keinen Anteil für {lRechnung.Umlage.Typ.Bezeichnung} gefunden",
                    Severity.Error);

                return 0;
            }
            else if (verbrauchAnteile.Count > 1)
            {
                notes.Add($"Mehr als einen Anteil für {lRechnung.Umlage.Typ.Bezeichnung} gefunden",
                    Severity.Error);

                return 0;
            }

            var verbrauchAnteil = verbrauchAnteile.First();

            if (verbrauchAnteil.Anteil.Count > 1)
            {
                notes.Add($"Verbrauch von Rechnung {lRechnung.Umlage.Typ.Bezeichnung} enthält mehr als einen Zählertypen",
                    Severity.Error);

                return 0;
            }
            return verbrauchAnteil.Anteil.First().Value;
        }

        public static List<Abrechnungseinheit> GetAbrechnungseinheiten(
            Vertrag vertrag,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            // Group up all Wohnungen sharing the same Umlage
            var einheiten = vertrag.Wohnung.Umlagen
                .GroupBy(umlage =>
                    [.. umlage.Wohnungen.Select(gr => gr.WohnungId).ToList()],
                    new SortedSetIntEqualityComparer())
                .ToList();
            // Then create Rechnungsgruppen for every single one of those groups with respective information to calculate the distribution
            return einheiten
                .Select(einheit => new Abrechnungseinheit(einheit.ToList(), vertrag, zeitraum, notes))
                .ToList();
        }

        private static List<Heizkostenberechnung> CalculateHeizkosten(
            List<BetriebskostenrechnungEntry> rechnungen,
            Wohnung wohnung,
            List<VerbrauchAnteil> verbrauchAnteile,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return [.. rechnungen.Select(rechnung => new Heizkostenberechnung(rechnung, wohnung, verbrauchAnteile, zeitraum, notes))];
        }

    }
}
