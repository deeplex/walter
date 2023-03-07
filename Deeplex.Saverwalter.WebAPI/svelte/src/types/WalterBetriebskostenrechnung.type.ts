import type { IWalterAnhang, WalterSelectionEntry, WalterWohnungEntry } from "$WalterTypes";

export type WalterBetriebskostenrechnungEntry = {
    id: number;
    betrag: number;
    betreffendesJahr: number;
    datum: string;
    notiz: string;
    typ: WalterSelectionEntry;
    umlage: WalterSelectionEntry;

    wohnungen: WalterWohnungEntry[];
} & IWalterAnhang
