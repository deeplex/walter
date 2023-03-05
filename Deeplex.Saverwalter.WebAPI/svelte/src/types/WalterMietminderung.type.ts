import type { WalterSelectionEntry } from "./WalterSelection.type";

export type WalterMietminderungEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    minderung: number;
    notiz: string;
    vertrag: WalterSelectionEntry;
}
