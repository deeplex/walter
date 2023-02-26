import type { PersonEntry } from "./person.type";

export type JuristischePersonEntry = {
    mitglieder: PersonEntry[];
} & PersonEntry