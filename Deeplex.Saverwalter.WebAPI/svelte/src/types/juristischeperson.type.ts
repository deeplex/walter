import type { KontaktListEntry } from "./kontaktlist.type";
import type { PersonEntry } from "./person.type";

export type JuristischePersonEntry = {
    mitglieder: KontaktListEntry[];
} & PersonEntry