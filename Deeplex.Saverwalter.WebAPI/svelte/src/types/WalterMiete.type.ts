import type { WalterSelectionEntry } from "./WalterSelection.type";

export type WalterMieteEntry = {
    id: number;
    betreffenderMonat: string;
    zahlungsdatum: string;
    betrag: number;
    vertrag: WalterSelectionEntry;
    notiz: string;
}
