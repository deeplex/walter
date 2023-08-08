import type {
    WalterSelectionEntry,
    WalterUmlageEntry,
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
    zaehler: WalterZaehlerEntry[];
    abrechnungseinheiten: WalterAbrechnungseinheit[];
    result: number;
    zeitraum: WalterZeitraum;
};

export type WalterBetriebskostenabrechnungNote = {
    message: string;
    severity: string;
};

export type WalterAbrechnungseinheit = {
    rechnungen: WalterRechnungEntry[],
    betragKalt: number;
    betragWarm: number;
    gesamtBetragKalt: number;
    gesamtBetragWarm: number;
    bezeichnung: string;
    gesamtWohnflaeche: number;
    gesamtNutzflaeche: number;
    gesamtEinheiten: number;
    wFZeitanteil: number;
    nFZeitanteil: number;
    nEZeitanteil: number;
    verbrauchAnteil: WalterVerbrauchAnteil[];
    personenZeitanteil: WalterPersonenZeitanteil[];
    heizkostenberechnungen: WalterHeizkostenberechnungEntry[];
    allgStromFaktor: number;
}

// TODO used where?
export type WalterVerbrauchAnteil = {
    umlage: WalterUmlageEntry;
    alleZaehler: {key: string, value: WalterVerbrauch[]},
    alleVerbrauch: {key: string, value: number},
    dieseZaehler: {key: string, value: WalterVerbrauch[]},
    dieseVerbrauch: {key: string, value: number},
    anteil: {key: string, value: number}
}

// TODO not active
export type WalterVerbrauch = {
    zaehler: WalterZaehlerEntry,
    endstand: WalterZaehlerstandEntry,
    anfangsstand: WalterZaehlerstandEntry,
    delta: number
}

export type WalterZeitraum = {
    nutzungsbeginn: Date;
    nutzungsende: Date;
    abrechnungsbeginn: Date;
    abrechnungsende: Date;
    abrechnungszeitraum: number;
    nutzungszeitraum: number;
    zeitanteil: number;
    jahr: number;
}

export type WalterPersonenZeitanteil = {
    beginn: Date;
    ende: Date;
    tage: number;
    personenzahl: number;
    gesamtPersonenzahl: number;
    anteil: number;
};

export type WalterRechnungEntry = {
    id: number,
    rechnungId: number,
    typ: string,
    typId: number,
    schluessel: string,
    gesamtBetrag: number,
    anteil: number,
    betrag: number
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
