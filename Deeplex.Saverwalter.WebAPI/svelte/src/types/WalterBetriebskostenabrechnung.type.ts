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

import type {
    WalterMieteEntry,
    WalterSelectionEntry,
    WalterVertragEntry,
    WalterWohnungEntry,
    WalterZaehlerEntry
} from '$walter/lib';

export type WalterBetriebskostenabrechnungEntry = {
    notes: WalterBetriebskostenabrechnungNote[];
    vermieter: WalterSelectionEntry;
    ansprechpartner: WalterSelectionEntry;
    mieter: WalterSelectionEntry[];
    vertrag: WalterSelectionEntry;
    gezahltMiete: number;
    kaltMiete: number;
    betragNebenkosten: number;
    bezahltNebenkosten: number;
    mietMinderung: number;
    nebenkostenMietminderung: number;
    kaltMietminderung: number;
    abrechnungseinheiten: WalterAbrechnungseinheit[];
    result: number;
    zeitraum: WalterZeitraum;
    zaehler: WalterZaehlerEntry[];
    wohnungen: WalterWohnungEntry[];
    vertraege: WalterVertragEntry[];
    mieten: WalterMieteEntry[];
};

export type WalterBetriebskostenabrechnungNote = {
    message: string;
    severity: string;
};

export type WalterAbrechnungseinheit = {
    rechnungen: WalterRechnungEntry[];
    betragKalt: number;
    betragWarm: number;
    gesamtBetragKalt: number;
    gesamtBetragWarm: number;
    bezeichnung: string;
    gesamtWohnflaeche: number;
    gesamtNutzflaeche: number;
    gesamtEinheiten: number;
    wfZeitanteil: number;
    nfZeitanteil: number;
    neZeitanteil: number;
    verbrauchAnteil: WalterVerbrauchAnteil[];
    personenZeitanteil: WalterPersonenZeitanteil[];
    heizkostenberechnungen: WalterHeizkostenberechnungEntry[];
    allgStromFaktor: number;
};

export type WalterVerbrauchAnteil = {
    umlage: WalterSelectionEntry;
    alleZaehler: { [key: string]: WalterVerbrauch[] };
    alleVerbrauch: { [key: string]: number };
    dieseZaehler: { [key: string]: WalterVerbrauch[] };
    dieseVerbrauch: { [key: string]: number };
    anteil: { [key: string]: number };
};

export type WalterVerbrauch = {
    zaehler: WalterSelectionEntry;
    delta: number;
};

export type WalterZeitraum = {
    nutzungsbeginn: Date;
    nutzungsende: Date;
    abrechnungsbeginn: Date;
    abrechnungsende: Date;
    abrechnungszeitraum: number;
    nutzungszeitraum: number;
    zeitanteil: number;
    jahr: number;
};

export type WalterPersonenZeitanteil = {
    beginn: Date;
    ende: Date;
    tage: number;
    personenzahl: number;
    gesamtPersonenzahl: number;
    anteil: number;
};

export type WalterRechnungEntry = {
    id: number;
    rechnungId: number;
    typ: string;
    typId: number;
    schluessel: string;
    gesamtBetrag: number;
    betragLetztesJahr: number;
    anteil: number;
    betrag: number;
    beschreibung: string;
};

export type WalterHeizkostenberechnungEntry = {
    gesamtBetrag: number;
    pauschalBetrag: number;
    tw: number;
    v: number;
    q: number;
    para7: number;
    para8: number;
    gesamtNutzflaeche: number;
    nfZeitanteil: number;
    heizkostenVerbrauchAnteil: number;
    warmwasserVerbrauchAnteil: number;
    para9_2: number;
    waermeAnteilNF: number;
    waermeAnteilVerb: number;
    warmwasserAnteilNF: number;
    warmwasserAnteilVerb: number;
    betrag: number;
};
