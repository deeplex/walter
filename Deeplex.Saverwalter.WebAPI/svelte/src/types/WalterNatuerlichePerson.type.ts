import type { WalterPersonEntry } from "$WalterTypes";

export type WalterNatuerlichePersonEntry = {
    vorname: string;
    nachname: string;
} & WalterPersonEntry