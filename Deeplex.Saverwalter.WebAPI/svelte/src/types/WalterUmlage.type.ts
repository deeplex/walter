import type { WalterAnhangEntry, WalterBetriebskostenrechnungEntry, WalterWohnungEntry } from "$WalterTypes";

export type WalterUmlageEntry = {
    id: number;
    notiz: string;
    beschreibung: string;
    wohnungenBezeichnung: string;
    // TODO => SelectionEntry
    zaehler: string;
    // TODO => SelectionEntry
    typ: string;
    // TODO => SelectionEntry
    schluessel: string;

    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
    wohnungen: WalterWohnungEntry[];
    anhaenge: WalterAnhangEntry[];
}