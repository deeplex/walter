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

using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Heizkostenberechnung
    {
        public double GesamtBetrag { get; }
        public double PauschalBetrag { get; }

        public double tw { get; }
        public double V { get; }
        public double Q { get; }

        public double Para7 { get; }
        public double Para8 { get; }

        public double GesamtNutzflaeche { get; }
        public double NFZeitanteil { get; }
        public double HeizkostenVerbrauchAnteil { get; }
        public double WarmwasserVerbrauchAnteil { get; }

        public double Para9_2 { get; }

        public double WaermeAnteilNF { get; }
        public double WaermeAnteilVerb { get; }
        public double WarmwasserAnteilNF { get; }
        public double WarmwasserAnteilVerb { get; }

        public double Betrag { get; }

        public Heizkostenberechnung(
            BetriebskostenrechnungEntry rechnung,
            Wohnung wohnung,
            List<VerbrauchAnteil> verbrauchAnteile,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            var lRechnung = rechnung.Rechnung;
            if (lRechnung == null)
            {
                notes.Add("Keine Rechnung für Heizkostenabrechnung gefunden.", Severity.Error);
                return;
            }

            GesamtBetrag = rechnung.Betrag;
            if (lRechnung.Umlage.HKVO is HKVO hkvo)
            {
                PauschalBetrag = rechnung.Betrag + rechnung.Betrag * hkvo.Strompauschale;
            }
            else
            {
                notes.Add("Warme Rechnung hat keine HKVO.", Severity.Error);
            }

            tw = 60;
            Para7 = lRechnung.Umlage.HKVO?.HKVO_P7 ?? 0.5; // HeizkostenV §7
            Para8 = lRechnung.Umlage.HKVO?.HKVO_P8 ?? 0.5; // HeizkostenV §8

            var warmwasserZaehlerAbrechnungseinheit = lRechnung.Umlage.Zaehler.Where(e => e.Typ == Zaehlertyp.Warmwasser).ToList();
            if (warmwasserZaehlerAbrechnungseinheit.Count == 0)
            {
                notes.Add("Keine Warmwasserzähler in Abrechnungseinheit gefunden.", Severity.Error);
            }

            var warmwasserZaehlerWohnung = warmwasserZaehlerAbrechnungseinheit.Where(z => z.Wohnung == wohnung).ToList();
            if (warmwasserZaehlerWohnung.Count == 0)
            {
                notes.Add("Keine Warmwasserzähler für Wohnung.", Severity.Error);
            }

            var gasAllgemeinZaehler = lRechnung.Umlage.Zaehler.Where(z => z.Wohnung == null && z.Typ == Zaehlertyp.Gas).ToList();

            if (gasAllgemeinZaehler.Count == 0)
            {
                notes.Add($"Notwendiger Allgemeinzähler Gas für Heizkosten ist nicht definiert.", Severity.Error);
                return;
            }


            var anteile = verbrauchAnteile.Where(anteil => anteil.Umlage == lRechnung.Umlage);
            var dieseAnteile = anteile.SelectMany(anteil => anteil.DieseZaehler.Values.SelectMany(value => value));
            var alleAnteile = anteile.SelectMany(anteil => anteil.AlleZaehler.Values.SelectMany(value => value));

            V = alleAnteile.Where(w => w.Zaehler.Typ == Zaehlertyp.Warmwasser).Sum(verbrauch => verbrauch.Delta);
            Q = Delta(gasAllgemeinZaehler, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende);

            var checkQ = alleAnteile.Where(w => w.Zaehler.Typ == Zaehlertyp.Gas).Sum(verbrauch => verbrauch.Delta);
            if (Q < checkQ)
            {
                notes.Add($"Gesamtzähler für Heizung zählt {Q} kWh, während die Summer der einzelnen Wärmemengenzähler {checkQ} kWh ist. " +
                    $"Der Allgemeinzähler muss mehr zählen als die Summe der einzelnen.", Severity.Error);
            }

            if (Q == 0)
            {
                notes.Add("Gesamtzähler steht auf 0.", Severity.Error);
            }

            Para9_2 = 2.5 * (V / Q) * (tw - 10); // TODO HeizkostenV §9

            if (Para9_2 > 1)
            {
                notes.Add("Heizkostenverteilung nach §9(2) ist über 100%.", Severity.Error);
            }

            if (Para9_2 <= 0)
            {
                notes.Add("Heizkostenverteilung nach $9(2) ist kleiner als 0%", Severity.Error);
            }

            GesamtNutzflaeche = lRechnung.Umlage.Wohnungen.Sum(w => w.Nutzflaeche);
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil;

            var q = dieseAnteile.Where(w => w.Zaehler.Typ == Zaehlertyp.Gas).Sum(verbrauch => verbrauch.Delta);
            HeizkostenVerbrauchAnteil = checkQ > 0 ? q / checkQ : 0;
            var v = dieseAnteile.Where(w => w.Zaehler.Typ == Zaehlertyp.Warmwasser).Sum(verbrauch => verbrauch.Delta);
            WarmwasserVerbrauchAnteil = V > 0 ? v / V : 0;

            WaermeAnteilNF = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * NFZeitanteil;
            WaermeAnteilVerb = PauschalBetrag * (1 - Para9_2) * (1 - Para7) * HeizkostenVerbrauchAnteil;
            WarmwasserAnteilNF = PauschalBetrag * Para9_2 * Para8 * NFZeitanteil;
            WarmwasserAnteilVerb = PauschalBetrag * Para9_2 * Para8 * WarmwasserVerbrauchAnteil;

            Betrag = WaermeAnteilNF + WaermeAnteilVerb + WarmwasserAnteilNF + WarmwasserAnteilVerb;
        }

        private static double Delta(IEnumerable<Zaehler> zaehlerList, DateOnly beginn, DateOnly ende)
        {
            var beginnValue = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                .LastOrDefault(l => l.Datum <= beginn && (beginn.DayNumber - l.Datum.DayNumber) < 30))
                .Where(zaehlerstand => zaehlerstand != null)
                .Select(zaehlerstand => zaehlerstand!)
                .Sum(w => w.Stand);

            var endeValue = zaehlerList.Select(z => z.Staende.OrderBy(s => s.Datum)
                .LastOrDefault(l => l.Datum <= ende && (ende.DayNumber - l.Datum.DayNumber) < 30))
                .Where(zaehlerstand => zaehlerstand != null)
                .Select(zaehlerstand => zaehlerstand!)
                .Sum(w => w.Stand);

            return endeValue - beginnValue;
        }
    }
}
