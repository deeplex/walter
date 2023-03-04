import type { WalterAdresseEntry, WalterAnhangEntry, WalterSelectionEntry, WalterZaehlerstandEntry } from "$WalterTypes";

export type WalterZaehlerEntry = {
    id: number;
    kennnummer: string;
    adresse: WalterAdresseEntry;
    typ: WalterSelectionEntry | undefined;
    allgemeinZaehler: WalterSelectionEntry | undefined;
    wohnung: WalterSelectionEntry | undefined;
    notiz: string;

    staende: WalterZaehlerstandEntry[];
    einzelzaehler: WalterZaehlerEntry[];
    anhaenge: WalterAnhangEntry[];

}