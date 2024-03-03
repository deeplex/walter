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
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.Utils;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Betriebskostenabrechnung
    {
        public List<Note> Notes { get; } = new List<Note>();
        public Kontakt Vermieter { get; } = null!;
        public Kontakt Ansprechpartner { get; }
        public List<Kontakt> Mieter { get; }
        public Vertrag Vertrag { get; }
        public double GezahlteMiete { get; }
        public double KaltMiete { get; }
        public double BetragNebenkosten { get; }
        public double BezahltNebenkosten { get; }
        public double Mietminderung { get; }
        public double NebenkostenMietminderung { get; }
        public double KaltMietminderung { get; }
        public Zeitraum Zeitraum { get; }
        public List<Abrechnungseinheit> Abrechnungseinheiten { get; }

        public double Result { get; }

        public Betriebskostenabrechnung(
            Vertrag vertrag,
            int jahr,
            DateOnly abrechnungsbeginn,
            DateOnly abrechnungsende)
        {
            Vertrag = vertrag;
            Zeitraum = new Zeitraum(jahr, vertrag);

            Vermieter = Vertrag.Wohnung.Besitzer!;

            Ansprechpartner = vertrag.Ansprechpartner ?? Vermieter;

            if (Vermieter == null)
            {
                Notes.Add(new Note(
                    $"Die Wohnung benötigt für die Abrechnung einen Ansprechpartner (Besitzer)",
                    Severity.Error));
            }

            Mieter = Vertrag.Mieter;


            if (Ansprechpartner.Adresse == null)
            {
                Notes.Add(new Note(
                    $"Die Adresse des Ansprechpartners {Ansprechpartner.Bezeichnung} ist notwendig für die Betriebskostenabrechnung.",
                    Severity.Error
                ));
            }

            GezahlteMiete = Mietzahlungen(vertrag, Zeitraum);
            KaltMiete = GetKaltMiete(vertrag, Zeitraum);

            if (!vertrag.Mieter.Any(mieter => mieter.Adresse != null))
            {
                Notes.Add(new Note(
                    $"Die Adresse mindestens eines Mieters ist notwendig für die Betriebskostenabrechnung.",
                    Severity.Error
                ));
            }

            Abrechnungseinheiten = Abrechnungseinheit.GetAbrechnungseinheiten(vertrag, Zeitraum, Notes);
            BetragNebenkosten = Abrechnungseinheiten.Sum(einheit => einheit.BetragKalt + einheit.BetragWarm);

            Mietminderung = GetMietminderung(vertrag, abrechnungsbeginn, abrechnungsende);
            NebenkostenMietminderung = BetragNebenkosten * Mietminderung;
            KaltMietminderung = KaltMiete * Mietminderung;
            BezahltNebenkosten = GezahlteMiete - KaltMiete + KaltMietminderung;
            Result = BezahltNebenkosten - BetragNebenkosten + NebenkostenMietminderung;

            Notes = Notes.Distinct().ToList();
        }
    }
}
