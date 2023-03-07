import type { IWalterAnhang, WalterSelectionEntry } from "$WalterTypes";

export type WalterErhaltungsaufwendungEntry = {
    id: number;
    betrag: number;
    datum: string;
    notiz: string;
    bezeichnung: string;
    wohnung: WalterSelectionEntry;
    aussteller: WalterSelectionEntry;
} & IWalterAnhang;