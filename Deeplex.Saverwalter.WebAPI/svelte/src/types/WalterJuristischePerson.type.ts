import type { WalterPersonEntry } from "$WalterTypes";

export type WalterJuristischePersonEntry = {
    mitglieder: WalterPersonEntry[];
} & WalterPersonEntry