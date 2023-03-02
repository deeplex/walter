import type { WalterAnhangEntry, WalterSelectionEntry, WalterWohnungEntry } from "$WalterTypes";

export type WalterBetriebskostenrechnungEntry = {
    id: number;
    betrag: number;
    betreffendesJahr: number;
    datum: string;
    notiz: string;
    // TODO
    // Hier muss eigentlich auf Umlagetyp geguckt werden, was dann die Wohnungen limitiert die reinkommen können.
    // Aber es gibt halt noch keine Umlagetypliste... 
    // typ: SelectionEntry;
    // wohnungen: SelectionEntry
    wohnungenBezeichnung: string;
    umlage: WalterSelectionEntry;

    wohnungen: WalterWohnungEntry[];
    anhaenge: WalterAnhangEntry[];
}
