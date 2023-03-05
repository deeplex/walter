import type { WalterSelectionEntry } from "./WalterSelection.type";

export type WalterVertragVersionEntry = {
    id: number;
    beginn: string;
    personenzahl: number;
    notiz: string;
    grundmiete: number;
    vertrag: WalterSelectionEntry;
}