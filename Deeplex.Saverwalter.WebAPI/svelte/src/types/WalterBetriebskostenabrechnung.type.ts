import type { WalterAdresseEntry, WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";

export type WalterBetriebskostenabrechnungEntry = {
    notes: WalterBetriebskostenabrechnungNote[];
    jahr: number;
    abrechnungsbeginn: Date;
    abrechnungsende: Date;
    vermieter: WalterSelectionEntry;
    ansprechpartner: WalterSelectionEntry;
    mieter: WalterSelectionEntry[];
    vertrag: WalterSelectionEntry;
    wohnung: WalterSelectionEntry;
    adresse: WalterAdresseEntry;
    gezahlt: number;
    kaltMiete: number;
    betragNebenkosten: number;
    bezahltNebenkosten: number;
    minderung: number;
    nebenkostenMinderung: number;
    kaltMinderung: number;
    nutzungsbeginn: Date;
    nutzungsende: Date;
    zaehler: WalterSelectionEntry[];
    abrechnungszeitspanne: number;
    nutzungszeitspanne: number;
    zeitanteil: number;
    gruppen: WalterBetriebskostenabrechnungsRechnungsgruppe[];
    result: number;
    allgStromFaktor: number;
}

export type WalterBetriebskostenabrechnungNote = {
    message: string;
    severity: string;
}

export type WalterBetriebskostenabrechnungPersonenZeitIntervall = {
    beginn: Date;
    ende: Date;
    tage: number;
    gesamtTage: number;
    personenzahl: number;
}

export type WalterBetriebskostenabrechnungsRechnungsgruppe = {
    umlagen: WalterUmlageEntry[];

    bezeichnung: string;
    gesamtWohnflaeche: number;
    wfZeitanteil: number;
    nfZeitanteil: number;
    gesamtNutzflaeche: number;
    gesamtEinheiten: number;
    nEZeitanteil: number;
    gesamtPersonenIntervall: WalterBetriebskostenabrechnungPersonenZeitIntervall[];
    personenIntervall: WalterBetriebskostenabrechnungPersonenZeitIntervall[];
    personenZeitanteil: WalterBetriebskostenabrechnungPersonenZeitIntervall[];
    heizkosten: WalterBetriebskostenabrechnungHeizkostenberechnungEntry[];
    gesamtBetragKalt: number;
    betragKalt: number;
    gesamtBetragWarm: number;
    betragWarm: number;
}

export type WalterBetriebskostenabrechnungsRechnungsgruppeEntry = {
    kostenpunkte: WalterBetriebskostenabrechnungKostenpunkt[]
} & WalterBetriebskostenabrechnungsRechnungsgruppe

export type WalterBetriebskostenabrechnungHeizkostenberechnungEntry = {
    betrag: number;
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
    kosten: number;
}

export type WalterBetriebskostenabrechnungKostenpunkt = {
    id: number; // just for rows
    typ: string;
    schluessel: string;
    nutzungsintervall: string;
    betrag: number;
    anteil: number;
    kosten: number;
}
