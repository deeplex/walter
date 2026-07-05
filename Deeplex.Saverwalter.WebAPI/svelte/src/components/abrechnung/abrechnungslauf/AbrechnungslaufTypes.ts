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

export type ZaehlerVerbrauchInfo = {
    zaehlerId: number;
    kennnummer: string;
    verbrauch: number;
    einheit: string;
};

export type NkAnteilInfo = {
    vertragId: number | null;
    bezeichnung: string;
    geplanterBetrag: number | null;
    gebuchterBetrag: number | null;
    anteilFaktor: number;
    nfZeitanteil: number;
    nutzungsbeginn: string;
    nutzungsende: string;
    heizVerbrauchAnteil: number | null;
    wwVerbrauchAnteil: number | null;
    heizZaehler: ZaehlerVerbrauchInfo[];
    wwZaehler: ZaehlerVerbrauchInfo[];
    verbrauchZaehler: ZaehlerVerbrauchInfo[];
};

/** Eingangswerte der §9(2)-Berechnung: P = 2,5 × V × (Tw − 10) / Q. */
export type P9DetailsInfo = {
    v: number;
    q: number;
    tw: number;
    allgemeinZaehler: string;
    qAnfangsdatum: string;
    qEnddatum: string;
    wwZaehler: ZaehlerVerbrauchInfo[];
};

export type NkZeileInfo = {
    umlageId: number;
    bezeichnung: string;
    beschreibung: string;
    schluessel: string;
    umlagetypId: number;
    buchungssatzId: string | null;
    betrag: number;
    betragLetztesJahr: number;
    istVollstaendigGebucht: boolean;
    anteile: NkAnteilInfo[];
    para9_2: number | null;
    p9Details: P9DetailsInfo | null;
    p7: number | null;
    p8: number | null;
    gesamtWaerme: number | null;
    gesamtWW: number | null;
    gesamtVerbrauch: number | null;
    verbrauchEinheit: string | null;
    istFehlend: boolean;
};

export type AbrechnungseinheitInfo = {
    wohnungNamen: string;
    nkZeilen: NkZeileInfo[];
    gesamtWohnflaeche: number;
    gesamtNutzflaeche: number;
    gesamtNutzeinheit: number;
    gesamtMiteigentumsanteile: number;
};

export type MietZeileInfo = {
    buchungsdatum: string;
    buchungsjahr: number;
    beschreibung: string;
    istSoll: boolean;
    betrag: number;
};

export type PersonenZeitanteilInfo = {
    beginn: string;
    ende: string;
    tage: number;
    personenzahl: number;
    gesamtPersonenzahl: number;
    anteil: number;
};

export type AbrechnungsresultatInfo = {
    abrechnungsresultatId: string;
    vertragId: number;
    wohnungId: number;
    wohnungBezeichnung: string;
    mieterBezeichnung: string;
    nutzungVon: string;
    nutzungBis: string | null;
    jahr: number;
    rechnungsbetrag: number;
    vorauszahlung: number;
    saldo: number;
    mietSaldo: number;
    kaltmieteSoll: number;
    wohnflaeche: number;
    mieten: MietZeileInfo[];
    personenZeitanteile: PersonenZeitanteilInfo[];
    gebuchterSaldo: number | null;
    abgesendet: boolean | null;
};

export type AbrechnungslaufGruppeResult = {
    gruppenBezeichnung: string;
    wohnungIds: number[];
    resultate: AbrechnungsresultatInfo[];
    abrechnungseinheiten: AbrechnungseinheitInfo[];
};

export function hkvoKosten(z: NkZeileInfo, a: NkAnteilInfo): number {
    const heizBetrag = z.betrag * (1 - z.para9_2!);
    const wwBetrag = z.betrag * z.para9_2!;
    // Verbrauchsunabhängiger Anteil (§7/§8) nach Nutzfläche.
    const vbHeiz =
        a.heizVerbrauchAnteil != null ? z.p7! * a.heizVerbrauchAnteil : 0;
    const nfHeiz = (1 - z.p7!) * a.nfZeitanteil;
    const vbWW = a.wwVerbrauchAnteil != null ? z.p8! * a.wwVerbrauchAnteil : 0;
    const nfWW = (1 - z.p8!) * a.nfZeitanteil;
    return heizBetrag * (vbHeiz + nfHeiz) + wwBetrag * (vbWW + nfWW);
}
