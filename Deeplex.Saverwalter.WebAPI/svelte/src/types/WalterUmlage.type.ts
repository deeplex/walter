import type {
    WalterBetriebskostenrechnungEntry,
    WalterSelectionEntry,
    WalterWohnungEntry
} from "$WalterTypes";

export type WalterUmlageEntry = {
    id: number;
    notiz: string;
    beschreibung: string;
    wohnungenBezeichnung: string;
    zaehler: string;
    typ: WalterSelectionEntry;
    schluessel: WalterSelectionEntry;
    selectedWohnungen: WalterSelectionEntry[];

    wohnungen: WalterWohnungEntry[];
    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
};