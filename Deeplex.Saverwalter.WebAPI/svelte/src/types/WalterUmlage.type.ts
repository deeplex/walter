import type { WalterAnhangEntry, WalterBetriebskostenrechnungEntry, WalterSelectionEntry, WalterWohnungEntry } from "$WalterTypes";

export type WalterUmlageEntry = {
    id: number;
    notiz: string;
    beschreibung: string;
    wohnungenBezeichnung: string;
    // TODO => SelectionEntry
    zaehler: string;
    typ: WalterSelectionEntry;
    schluessel: WalterSelectionEntry;
    selectedWohnungen: WalterSelectionEntry[];

    wohnungen: WalterWohnungEntry[];
    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
    anhaenge: WalterAnhangEntry[];
}