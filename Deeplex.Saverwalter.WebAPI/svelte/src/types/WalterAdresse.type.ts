import type { WalterZaehlerEntry, WalterWohnungEntry, WalterPersonEntry } from "$WalterTypes";

export type WalterAdresseEntry = {
    strasse: string;
    hausnummer: string;
    postleitzahl: string;
    stadt: string;
    anschrift: string;
    notiz: string;

    wohnungen: WalterWohnungEntry[];
    kontakte: WalterPersonEntry[];
    zaehler: WalterZaehlerEntry[];
}