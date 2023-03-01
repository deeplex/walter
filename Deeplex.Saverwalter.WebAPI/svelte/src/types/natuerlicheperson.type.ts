import type { PersonEntry } from "$types";

export type NatuerlichePersonEntry = {
    vorname: string;
    nachname: string;
} & PersonEntry