import type { WalterSelectionEntry } from "./WalterSelection.type";

export type WalterZaehlerstandEntry = {
    id: number;
    stand: number;
    datum: string;
    einheit: string;
    zaehler: WalterSelectionEntry;
    notiz: string;
}