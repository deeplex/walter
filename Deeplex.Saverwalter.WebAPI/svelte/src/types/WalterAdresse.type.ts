import type { WalterWohnungEntry } from "./WalterWohnung.type";

export type WalterAdresseEntry = {
    strasse: string;
    hausnummer: string;
    postleitzahl: string;
    stadt: string;
    anschrift: string;

    wohnungen: WalterWohnungEntry[];
}