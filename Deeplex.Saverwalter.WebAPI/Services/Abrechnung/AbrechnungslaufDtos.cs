// Copyright (c) 2023-2026 Kai Lawrence
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

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    /// <summary>Verbrauch eines einzelnen Zählers für eine Partei in einem Abrechnungszeitraum.</summary>
    public class ZaehlerVerbrauchInfo
    {
        public int ZaehlerId { get; init; }
        public string Kennnummer { get; init; } = "";
        public decimal Verbrauch { get; init; }
        /// <summary>"kWh" für Gas/Wärme-Zähler, "m³" für Warmwasser-Zähler.</summary>
        public string Einheit { get; init; } = "";
    }

    /// <summary>Einzelner NK-Anteil einer Betriebskostenrechnung je Partei.</summary>
    public class NkAnteilInfo
    {
        public int? VertragId { get; init; }
        public string Bezeichnung { get; init; } = "";
        public decimal? GeplanterBetrag { get; init; }
        public decimal? GebuchterBetrag { get; init; }
        /// <summary>Anteilsfaktor 0–1; proportionaler Anteil dieser Partei an der Rechnung.</summary>
        public decimal AnteilFaktor { get; init; }
        /// <summary>Nutzflächen-Zeitanteil (0–1) – Basis der verbrauchsunabhängigen §7/§8-Quote.</summary>
        public decimal NfZeitanteil { get; init; }
        public DateOnly Nutzungsbeginn { get; init; }
        public DateOnly Nutzungsende { get; init; }
        /// <summary>Individueller Wärme-Verbrauchsanteil (0–1). Nur für HKVO-Umlagen.</summary>
        public decimal? HeizVerbrauchAnteil { get; init; }
        /// <summary>Individueller Warmwasser-Verbrauchsanteil (0–1). Nur für HKVO-Umlagen.</summary>
        public decimal? WWVerbrauchAnteil { get; init; }
        /// <summary>Wärme-Einzelzähler dieser Partei. Nur für HKVO-Umlagen.</summary>
        public List<ZaehlerVerbrauchInfo> HeizZaehler { get; init; } = [];
        /// <summary>Warmwasser-Einzelzähler dieser Partei. Nur für HKVO-Umlagen.</summary>
        public List<ZaehlerVerbrauchInfo> WwZaehler { get; init; } = [];
        /// <summary>Verbrauchszähler dieser Partei. Nur für kalte verbrauchsbasierte Umlagen.</summary>
        public List<ZaehlerVerbrauchInfo> VerbrauchZaehler { get; init; } = [];
    }

    /// <summary>Detailzeile je (Umlage, Rechnung) in der Gruppenansicht.</summary>
    public class NkZeileInfo
    {
        public int UmlageId { get; init; }
        public string Bezeichnung { get; init; } = "";
        public string Beschreibung { get; init; } = "";
        public string Schluessel { get; init; } = "";
        public int UmlagetypId { get; init; }
        public Guid? BuchungssatzId { get; init; }
        public decimal Betrag { get; init; }
        public decimal BetragLetztesJahr { get; init; }
        public bool IstVollstaendigGebucht { get; init; }
        public List<NkAnteilInfo> Anteile { get; init; } = [];
        /// <summary>Gesetzt wenn diese Zeile eine HKVO-Umlage (warme Betriebskosten) ist.</summary>
        public decimal? Para9_2 { get; init; }
        public decimal? P7 { get; init; }
        public decimal? P8 { get; init; }
        /// <summary>Gesamtwärmeverbrauch aller Parteien in kWh. Nur für HKVO-Umlagen.</summary>
        public decimal? GesamtWaerme { get; init; }
        /// <summary>Gesamtwarmwasserverbrauch aller Parteien in m³. Nur für HKVO-Umlagen.</summary>
        public decimal? GesamtWW { get; init; }
        /// <summary>Gesamtverbrauch aller Parteien. Nur für kalte verbrauchsbasierte Umlagen.</summary>
        public decimal? GesamtVerbrauch { get; init; }
        /// <summary>Einheit des Gesamtverbrauchs (z.B. "m³", "kWh"). Nur für kalte verbrauchsbasierte Umlagen.</summary>
        public string? VerbrauchEinheit { get; init; }
        /// <summary>True wenn diese Umlage keine Buchungen für das Abrechnungsjahr hat.</summary>
        public bool IstFehlend { get; init; }
    }

    /// <summary>Zusammenfassung je Wohnungskombination (Umlage-Einheit).</summary>
    public class AbrechnungseinheitInfo
    {
        public string WohnungNamen { get; init; } = "";
        public List<NkZeileInfo> NkZeilen { get; init; } = [];
        public decimal GesamtWohnflaeche { get; init; }
        public decimal GesamtNutzflaeche { get; init; }
        public decimal GesamtNutzeinheit { get; init; }
        public decimal GesamtMiteigentumsanteile { get; init; }
    }

    /// <summary>Miet-Buchungszeile (Soll oder Haben) auf dem MietBuchungskonto eines Vertrags.</summary>
    public class MietZeileInfo
    {
        public DateOnly Buchungsdatum { get; init; }
        public string Beschreibung { get; init; } = "";
        public bool IstSoll { get; init; }
        public decimal Betrag { get; init; }
    }

    /// <summary>Personenzeitanteil-Intervall für die Darstellung im Abrechnungsresultat.</summary>
    public class PersonenZeitanteilInfo
    {
        public DateOnly Beginn { get; init; }
        public DateOnly Ende { get; init; }
        public int Tage { get; init; }
        public int Personenzahl { get; init; }
        public int GesamtPersonenzahl { get; init; }
        public decimal Anteil { get; init; }
    }

    /// <summary>Resultate je Vertrag oder Eigenanteil (Leerstand) sowie NK-Einheiten einer Gruppe.</summary>
    public class AbrechnungslaufGruppeResult
    {
        public string GruppenBezeichnung { get; init; } = "";
        public List<int> WohnungIds { get; init; } = [];
        public List<AbrechnungsresultatInfo> Resultate { get; init; } = [];
        public List<AbrechnungseinheitInfo> Abrechnungseinheiten { get; init; } = [];
        public List<string> Warnungen { get; init; } = [];
    }

    /// <summary>
    /// Abrechnungsergebnis je Vertrag oder Wohnung (Eigenanteil).
    /// <see cref="VertragId"/> ist null für Eigenanteil-/Leerstand-Positionen.
    /// </summary>
    public class AbrechnungsresultatInfo
    {
        public Guid AbrechnungsresultatId { get; init; }
        public int? VertragId { get; init; }
        public int WohnungId { get; init; }
        public string WohnungBezeichnung { get; init; } = "";
        public string MieterBezeichnung { get; init; } = "";
        public DateOnly NutzungVon { get; init; }
        public DateOnly? NutzungBis { get; init; }
        public int Jahr { get; init; }
        public decimal Rechnungsbetrag { get; init; }
        public decimal Vorauszahlung { get; init; }
        public decimal Saldo { get; init; }
        public decimal MietSaldo { get; init; }
        public decimal KaltmieteSoll { get; init; }
        public decimal Wohnflaeche { get; init; }
        public decimal Nutzflaeche { get; init; }
        public int Nutzeinheiten { get; init; }
        public List<MietZeileInfo> Mieten { get; init; } = [];
        public List<PersonenZeitanteilInfo> PersonenZeitanteile { get; init; } = [];
        /// <summary>
        /// Bereits gebuchter Rechnungsbetrag, oder null wenn noch nicht gebucht.
        /// Weicht von <see cref="Rechnungsbetrag"/> ab → Buchung ist veraltet.
        /// </summary>
        public decimal? GebuchtesAbrechnungsResultat { get; set; }
        public bool? Abgesendet { get; init; }
    }

    /// <summary>Vollständige Vorschau-/Buchungsantwort über mehrere Gruppen hinweg.</summary>
    public class AbrechnungslaufResult
    {
        public List<AbrechnungslaufGruppeResult> Gruppen { get; init; } = [];
        public List<string> Warnungen { get; init; } = [];
    }
}
