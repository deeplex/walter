import type {
    WalterMieteEntry,
    WalterSelectionEntry,
    WalterUmlageEntry,
    WalterVertragEntry,
    WalterWohnungEntry,
    WalterZaehlerEntry,
    WalterZaehlerstandEntry
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
