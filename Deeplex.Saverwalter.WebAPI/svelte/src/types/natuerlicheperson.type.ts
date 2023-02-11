import type { PersonEntry } from "./person.type";

export type NatuerlichePersonEntry = {
    vorname: string;
    nachname: string;
} & PersonEntry