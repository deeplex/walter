import type { WalterPersonEntry, WalterSelectionEntry } from "$WalterTypes";

export type WalterJuristischePersonEntry = {
    selectedMitglieder: WalterSelectionEntry[];

    mitglieder: WalterPersonEntry[];
} & WalterPersonEntry