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
    public class Abrechnungseinheit
    {
        public Dictionary<Umlage, Betriebskostenrechnung?> Rechnungen { get; } = new();
        public double BetragKalt { get; }
        public double BetragWarm { get; }
        public double GesamtBetragKalt { get; }
        public double GesamtBetragWarm { get; }
        public string Bezeichnung { get; }
        public double GesamtWohnflaeche { get; }
        public double GesamtNutzflaeche { get; }
        public int GesamtEinheiten { get; }
        public double WFZeitanteil { get; }
        public double NFZeitanteil { get; }
        public double NEZeitanteil { get; }
        public List<VerbrauchAnteil> VerbrauchAnteile { get; }
        public List<PersonenZeitanteil> PersonenZeitanteile { get; }
        public List<Heizkostenberechnung> Heizkostenberechnungen { get; }
        public double AllgStromFaktor { get; }

        public Abrechnungseinheit(List<Umlage> umlagen, Vertrag vertrag, Zeitraum zeitraum, List<Note> notes)
        {
            var wohnungen = umlagen.First().Wohnungen.ToList();
            Bezeichnung = umlagen.First().GetWohnungenBezeichnung();

            GesamtWohnflaeche = wohnungen.Sum(w => w.Wohnflaeche);
            GesamtNutzflaeche = wohnungen.Sum(w => w.Nutzflaeche);
            GesamtEinheiten = wohnungen.Sum(w => w.Nutzeinheit);

            var wohnung = vertrag.Wohnung;
            WFZeitanteil = wohnung.Wohnflaeche / GesamtWohnflaeche * zeitraum.Zeitanteil;
            NFZeitanteil = wohnung.Nutzflaeche / GesamtNutzflaeche * zeitraum.Zeitanteil;
            NEZeitanteil = (double)wohnung.Nutzeinheit / GesamtEinheiten * zeitraum.Zeitanteil;

            foreach (var umlage in umlagen)
            {
                Rechnungen[umlage] = umlage.Betriebskostenrechnungen
                    .SingleOrDefault(rechnung => rechnung.BetreffendesJahr == zeitraum.Jahr);

                if (Rechnungen[umlage] == null)
                {
                    notes.Add($"Keine Rechnung für {umlage.Typ.Bezeichnung} gefunden.", Severity.Warning);
                }
            }

            var rechnungenKalt = Rechnungen
                .Where(e => e.Key.HKVO == null && e.Value != null)
                .Select(e => e.Value!)
                .ToList();
            var rechnungenWarm = Rechnungen
                .Where(e => e.Key.HKVO != null && e.Value != null)
                .Select(e => e.Value!)
                .ToList();

            PersonenZeitanteile = PersonenZeitanteil.GetPersonenZeitanteile(vertrag, wohnungen, zeitraum);
            VerbrauchAnteile = VerbrauchAnteil.GetVerbrauchAnteile(umlagen, vertrag, zeitraum, notes);

            Heizkostenberechnungen = CalculateHeizkosten(rechnungenWarm, wohnung, VerbrauchAnteile, zeitraum, notes);
            BetragWarm = Heizkostenberechnungen.Sum(heizkosten => heizkosten.Betrag);

            rechnungenWarm.ForEach(warmeRechnung =>
            {
                if (warmeRechnung.Umlage.HKVO is HKVO hkvo)
                {
                    var stromRechnung = rechnungenKalt.FirstOrDefault(kalteRechnung =>
                        kalteRechnung.Umlage.UmlageId == hkvo.Betriebsstrom.UmlageId);

                    if (stromRechnung == null)
                    {
                        notes.Add("Keine Stromrechnung für Heizung gefunden", Severity.Error);
                        return;
                    }

                    var delta = warmeRechnung.Betrag * hkvo.Strompauschale;
                    // Das wird bereits in der Heizkostenberechnung gemacht und als Pauschalbetrag verbucht.
                    // Für die Stromrechnung allerdings muss der Teil noch abgezogen werden.
                    // warmeRechnung.Betrag += delta;
                    stromRechnung.Betrag -= delta;

                    if (stromRechnung.Betrag < 0)
                    {
                        notes.Add($"Pauschale der Heizkosten ({hkvo.Strompauschale:N2}%) ist mehr als Allgemeinstromrechnung {stromRechnung.Betrag:N2}€", Severity.Warning);
                    }
                }
            });

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
                Umlageschluessel.NachPersonenzahl => PersonenZeitanteile.Sum(anteil => anteil.Anteil),
                Umlageschluessel.NachVerbrauch => VerbrauchAnteile
                    .SingleOrDefault(anteil => anteil.Umlage == umlage)?.Anteil?.Sum(a => a.Value) ?? 0,
                _ => 0,
            };
        }

        private double GetSum(List<Betriebskostenrechnung> rechnungen, List<Note> notes)
        {
            var PersZeitanteil = PersonenZeitanteile.Sum(z => z.Anteil);

            return rechnungen.Sum(rechnung => rechnung.Umlage.Schluessel switch
            {
                Umlageschluessel.NachNutzeinheit => rechnung.Betrag * NEZeitanteil,
                Umlageschluessel.NachWohnflaeche => rechnung.Betrag * WFZeitanteil,
                Umlageschluessel.NachNutzflaeche => rechnung.Betrag * NFZeitanteil,
                Umlageschluessel.NachPersonenzahl => rechnung.Betrag * PersZeitanteil,
                Umlageschluessel.NachVerbrauch => rechnung.Betrag * GetVerbrauchAnteil(rechnung, notes),
                _ => 0
            });
        }

        private double GetVerbrauchAnteil(Betriebskostenrechnung rechnung, List<Note> notes)
        {
            var verbrauchAnteile = VerbrauchAnteile.Where(anteil => anteil.Umlage == rechnung.Umlage).ToList();
            if (verbrauchAnteile.Count == 0)
            {
                notes.Add($"Keinen Anteil für {rechnung.Umlage.Typ.Bezeichnung} gefunden",
                    Severity.Error);

                return 0;
            }
            else if (verbrauchAnteile.Count > 1)
            {
                notes.Add($"Mehr als einen Anteil für {rechnung.Umlage.Typ.Bezeichnung} gefunden",
                    Severity.Error);

                return 0;
            }

            var verbrauchAnteil = verbrauchAnteile.First();

            if (verbrauchAnteil.Anteil.Count > 1)
            {
                notes.Add($"Verbrauch von Rechnung {rechnung.Umlage.Typ.Bezeichnung} enthält mehr als einen Zählertypen",
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
                    new SortedSet<int>(umlage.Wohnungen.Select(gr => gr.WohnungId).ToList()),
                    new SortedSetIntEqualityComparer())
                .ToList();
            // Then create Rechnungsgruppen for every single one of those groups with respective information to calculate the distribution
            return einheiten
                .Select(einheit => new Abrechnungseinheit(einheit.ToList(), vertrag, zeitraum, notes))
                .ToList();
        }

        private static List<Heizkostenberechnung> CalculateHeizkosten(
            List<Betriebskostenrechnung> rechnungen,
            Wohnung wohnung,
            List<VerbrauchAnteil> verbrauchAnteile,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return rechnungen
                .Select(rechnung => new Heizkostenberechnung(rechnung, wohnung, verbrauchAnteile, zeitraum, notes))
                .ToList();
        }

    }
}
